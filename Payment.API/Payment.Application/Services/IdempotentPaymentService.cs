using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Payment.Application.Persistence;
using Payment.Domain.Models;
using Payment.Domain.Repositories;
using Polly;
using Polly.Retry;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace Payment.Application.Services
{
    public class IdempotentPaymentService
    {
        private readonly PaymentDbContext _db;
        private readonly IPaymentGatewayClient _gatewayClient;
        private readonly ILogger<IdempotentPaymentService> _logger;
        private readonly AsyncRetryPolicy _retryPolicy;

        public IdempotentPaymentService(
            PaymentDbContext db,
            IPaymentGatewayClient gatewayClient,
            ILogger<IdempotentPaymentService> logger)
        {
            _db = db;
            _gatewayClient = gatewayClient;
            _logger = logger;
            // Définition de la politique Polly : Retry 3 fois avec Backoff exponentiel (2s, 4s, 8s)
            _retryPolicy = Policy
                .Handle<HttpRequestException>()
                .Or<TimeoutException>()
                .WaitAndRetryAsync(
                    retryCount: 3,
                    sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
                    onRetry: (exception, timeSpan, retryCount, context) =>
                    {
                        _logger.LogWarning(exception, "Erreur transitoire détectée. Tentative de retry n°{RetryCount} dans {TimeSpan}s", retryCount, timeSpan.TotalSeconds);
                    });
        }
        public async Task<PaymentResult> ProcessPaymentAsync(PaymentRequest request)
        {
            // 1. Vérifier si la transaction existe déjà en base
            var existing = await _db.ProcessedTransactions
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.TransactionId == request.TransactionId);
            if (existing != null)
            {
                if (existing.Status == "Completed")
                {
                    _logger.LogInformation("Doublon détecté : La transaction {TransactionId} a déjà été traitée avec succès.", request.TransactionId);
                    return JsonSerializer.Deserialize<PaymentResult>(existing.ResponseData!)!;
                }

                if (existing.Status == "Pending")
                {
                    _logger.LogWarning("La transaction {TransactionId} est actuellement en cours de traitement par une autre instance.", request.TransactionId);
                    throw new InvalidOperationException("Transaction déjà en cours de traitement.");
                }
            }
            // 2. Marquer la transaction comme 'Pending' en base (Insert initial)
            var transactionEntity = new ProcessedTransactionEntity
            {
                TransactionId = request.TransactionId,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _db.ProcessedTransactions.Add(transactionEntity);
            try
            {
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                // Gestion de la concurrence extrême : si un autre thread a inséré le même ID entre temps
                _logger.LogWarning("Concurrence détectée sur l'insertion de la transaction {TransactionId}.", request.TransactionId);
                throw new InvalidOperationException("Cette transaction est déjà en cours de traitement.");
            }

            // 3. Exécuter l'appel bancaire protégé par la politique de Retry (Polly)
            try
            {
                var result = await _retryPolicy.ExecuteAsync(async () =>
                {
                    return await _gatewayClient.ChargeAsync(request);
                });

                // 4. Mettre à jour le statut en 'Completed' et sauvegarder le résultat
                transactionEntity.Status = "Completed";
                transactionEntity.ResponseData = JsonSerializer.Serialize(result);
                transactionEntity.UpdatedAt = DateTime.UtcNow;

                _db.ProcessedTransactions.Update(transactionEntity);
                await _db.SaveChangesAsync();

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Échec définitif du paiement pour la transaction {TransactionId} après les retries.", request.TransactionId);

                // Marquer comme 'Failed' pour autoriser potentiellement un nouveau retry ultérieur si souhaité
                transactionEntity.Status = "Failed";
                transactionEntity.ResponseData = JsonSerializer.Serialize(new { error = ex.Message });
                transactionEntity.UpdatedAt = DateTime.UtcNow;

                _db.ProcessedTransactions.Update(transactionEntity);
                await _db.SaveChangesAsync();

                throw;
            }
        }
    }
}