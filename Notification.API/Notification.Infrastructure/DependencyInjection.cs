using Notification.Infrastructure.Repositories;
using System;
using System.Collections.Generic;
using System.Text;

namespace Notification.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // Cosmos DB Client — Singleton car il gère lui-même le connection pool Même principe que IConnectionMultiplexer pour Redis

            var cosmosConnectionString = configuration["CosmosDb:ConnectionString"]
                ?? throw new InvalidOperationException("CosmosDb connection string not configured.");

            services.AddSingleton(new CosmosClient(
                cosmosConnectionString,
                new CosmosClientOptions
                {
                    // Sérialisation en camelCase pour les documents JSON
                    SerializerOptions = new CosmosSerializationOptions
                    {
                        PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
                    }
                }));

            services.AddScoped<INotificationRepository, NotificationRepository>();

            return services;
        }


    }
}
