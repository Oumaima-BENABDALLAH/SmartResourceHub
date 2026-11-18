using System;
using System.Collections.Generic;
using System.Text;

namespace Notification.Domain.Entities
{
    public  class NotificationDocument
    {

        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string UserId { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;

        // { "resourceName": "Salle A", "startTime": "2026-07-01" }
        public Dictionary<string, string> Metadata { get; set; } = new();

        public NotificationStatus Status { get; set; } = NotificationStatus.Pending;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? SentAt { get; set; }

        // Cosmos DB TTL — suppression automatique après 90 jours -1 = pas de suppression automatique
        public int Ttl { get; set; } = 90 * 24 * 3600; // 90 jours en secondes

    }
    public enum NotificationStatus
    {
        Pending = 0,   // En attente d'envoi
        Sent = 1,      // Envoyée avec succès
        Failed = 2     // Échec d'envoi
    }
}
