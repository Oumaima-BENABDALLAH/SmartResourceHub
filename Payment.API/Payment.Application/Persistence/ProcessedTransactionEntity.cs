using System;
using System.Collections.Generic;
using System.Text;

namespace Payment.Application.Persistence
{
    public class ProcessedTransactionEntity
    {
        public string TransactionId { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty; // 'Pending', 'Completed', 'Failed'
        public string? ResponseData { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
