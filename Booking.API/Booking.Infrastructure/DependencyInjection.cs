using Booking.Application.Common.Interfaces;
using Booking.Application.Sagas;
using Booking.Domain.Repositories;
using Booking.Infrastructure.Consumers;
using Booking.Infrastructure.Persistence;
using Booking.Infrastructure.Persistence.Repositories;
using Catalog.API.Protos;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Booking.Infrastructure
{
    //Configuration centralisée de l'injection de dépendances pour cette couche. Déclarée en 'static' pour permettre l'utilisation de méthodes d'extension.
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
         this IServiceCollection services,
         IConfiguration configuration)
        {
            //  EF Core : enregistre le contexte de base de données BookingDbContext pour utiliser SQL Server.
            services.AddDbContext<BookingDbContext>(options =>
                options.UseSqlServer(
                    configuration.GetConnectionString("BookingDb"),
                    b => b.MigrationsAssembly(typeof(BookingDbContext).Assembly.FullName)));

            services.AddScoped<IBookingRepository, BookingRepository>();

            services.AddScoped<IUnitOfWork>(provider =>
                provider.GetRequiredService<BookingDbContext>());

            //  gRPC Client vers Catalog.API 
            services.AddGrpcClient<CatalogService.CatalogServiceBase>(o =>
            {
                o.Address = new Uri(configuration["CatalogGrpc:Url"] ?? "https://localhost:5133");
            });

            //  MassTransit + RabbitMQ + Saga + Consumers 
            services.AddMassTransit(x =>
            {
                x.SetKebabCaseEndpointNameFormatter(); // Formate les noms des files d'attente (queues) au format kebab-case ex: booking-confirmed

                //Enregistre les consommateurs de messages (BookingConfirmedConsumer et BookingRejectedConsumer)
                x.AddConsumer<BookingConfirmedConsumer>(); 
                x.AddConsumer<BookingRejectedConsumer>();

                //Configure une machine à états (BookingStateMachine) pour gérer des flux de travail complexes (transactions distribuées) en utilisant un stockage en mémoire ( stockage en RAM , stockage temporaire )
                x.AddSagaStateMachine<BookingStateMachine, BookingState>()
                    .InMemoryRepository();

                //Configure le broker de messages RabbitMQ en récupérant l'hôte, l'utilisateur et le mot de passe  depuis epuis la configuration

                x.UsingRabbitMq((context, cfg) =>
                {
                    var rabbitHost = configuration["RabbitMq:Host"] ?? "localhost";  // hote
                    var rabbitUser = configuration["RabbitMq:Username"] ?? "guest"; // utilisateur
                    var rabbitPass = configuration["RabbitMq:Password"] ?? "guest"; // mdp

                    cfg.Host(rabbitHost, "/", h =>
                    {
                        h.Username(rabbitUser);
                        h.Password(rabbitPass);
                    });

                    cfg.ConfigureEndpoints(context);
                });
            });

            return services;
        }
    }
}
