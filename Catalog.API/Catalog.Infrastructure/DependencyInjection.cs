using Catalog.Application.Common.Interfaces;
using Catalog.Domain.Repositories;
using Catalog.Infrastructure.Persistence;
using Catalog.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Catalog.Infrastructure
{
    //Configuration centralisée de l'injection de dépendances pour cette couche. Déclarée en 'static' pour permettre l'utilisation de méthodes d'extension.
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {


            //  EF Core : enregistre le contexte de base de données CatalogDbContext pour utiliser SQL Server.
            services.AddDbContext<CatalogDbContext>(options =>
                options.UseSqlServer(
                    configuration.GetConnectionString("CatalogDb"),
                    b => b.MigrationsAssembly(typeof(CatalogDbContext).Assembly.FullName)));

            services.AddScoped<IResourceRepository, ResourceRepository>();
            services.AddScoped<IUnitOfWork>(provider =>
                           provider.GetRequiredService<CatalogDbContext>());
    

            return services;
        }
    }
}
