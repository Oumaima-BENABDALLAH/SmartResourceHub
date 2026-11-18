using System;
using System.Collections.Generic;
using System.Text;

namespace Payment.Domain.Models
{
    public class PaymentRequest
    {
        public string TransactionId { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "EUR";
        public string CardNumber { get; set; } = string.Empty;
    }

    public class PaymentResult
    {
        public string TransactionId { get; set; } = string.Empty;
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
        public DateTime ProcessedAt { get; set; }
    }
}
