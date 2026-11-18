using Booking.Application.Common.Behaviors;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Booking.Application
{

    // Ce fichier est une classe d'extension qui regroupe et enregistre automatiquement 
    // tous les services de la couche Application (MediatR, AutoMapper, validateurs) 
    // dans le catalogue .NET, permettant ainsi de garder le fichier Program.cs propre.

    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            // Récupère le projet compilé (l'Assembly) actuel 
            var assembly = typeof(DependencyInjection).Assembly;

            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));
            services.AddAutoMapper(config => config.AddMaps(assembly));
            services.AddValidatorsFromAssembly(assembly);

            services.AddTransient(
                typeof(IPipelineBehavior<,>),
                typeof(ValidationBehavior<,>));

            return services;
        }
    }

    
}
