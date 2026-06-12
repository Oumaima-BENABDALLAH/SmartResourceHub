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
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
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
