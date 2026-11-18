using Microsoft.Extensions.Configuration;
using Notification.Domain.Entities;
using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;


namespace Notification.Infrastructure.Repositories
{

    public interface INotificationRepository
    {
        Task SaveAsync(NotificationDocument document, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<NotificationDocument>> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default);
        Task UpdateStatusAsync(string id, string userId, NotificationStatus status, CancellationToken cancellationToken = default);
    }

    public class NotificationRepository : INotificationRepository
    {
        private readonly Container _container;

        public NotificationRepository(CosmosClient cosmosClient, IConfiguration configuration)
        {
            var databaseName = configuration["CosmosDb:DatabaseName"] ?? "NotificationDb";
            var containerName = configuration["CosmosDb:ContainerName"] ?? "Notifications";

            // Accès au container Cosmos DB
            _container = cosmosClient.GetContainer(databaseName, containerName);
        }

        public async Task SaveAsync( NotificationDocument document,CancellationToken cancellationToken = default)
        {
   
            await _container.UpsertItemAsync(
                document,
                new PartitionKey(document.UserId),
                cancellationToken: cancellationToken);
        }


        public async Task<IReadOnlyList<NotificationDocument>> GetByUserIdAsync(string userId,CancellationToken cancellationToken = default)
        {
            // Query Cosmos DB — ressemble à SQL mais c'est du NoSQL
            var query = new QueryDefinition(
                "SELECT * FROM c WHERE c.userId = @userId ORDER BY c.createdAt DESC")
                .WithParameter("@userId", userId);

            var results = new List<NotificationDocument>();

            // FeedIterator = curseur pour paginer les résultats
            using var iterator = _container.GetItemQueryIterator<NotificationDocument>(query);

            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync(cancellationToken);
                results.AddRange(response);
            }

            return results;
        }

        public async Task UpdateStatusAsync(string id,string userId, NotificationStatus status,CancellationToken cancellationToken = default)
        {
          
            var patchOperations = new[]
            {
            PatchOperation.Set("/status", status.ToString()),
            PatchOperation.Set("/sentAt", DateTime.UtcNow) };

            await _container.PatchItemAsync<NotificationDocument>(
                id,
                new PartitionKey(userId),
                patchOperations,
                cancellationToken: cancellationToken);
        }


    }
}
