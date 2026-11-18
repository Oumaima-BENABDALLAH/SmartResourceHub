using Microsoft.Extensions.Logging;
using Payment.Domain.Models;
using Payment.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Text;

namespace Payment.Application.Services
{
    public class PaymentGatewayClient : IPaymentGatewayClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<PaymentGatewayClient> _logger;

        public PaymentGatewayClient(HttpClient httpClient, ILogger<PaymentGatewayClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<PaymentResult> ChargeAsync(PaymentRequest request)
        {
            // Simulation d'un appel réseau vers la passerelle bancaire tierce
            _logger.LogInformation("Appel de la passerelle bancaire pour la transaction {TransactionId}...", request.TransactionId);
            await Task.Delay(500);
            return new PaymentResult
            {
                TransactionId = request.TransactionId,
                IsSuccess = true,
                Message = "Paiement validé avec succès par la banque.",
                ProcessedAt = DateTime.UtcNow
            };

        }
    }
}
