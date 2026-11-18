using Booking.Application.Sagas.Events;
using Booking.Domain.Repositories;
using Booking.Infrastructure.Persistence;
using MassTransit;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Booking.Infrastructure.Consumers
{
    // un Consumer(consommateur) MassTransit.Son rôle est d'écouter un message asynchrone (un événement) provenant d'un bus de messages(comme RabbitMQ)
    // pour réagir lorsqu'une réservation est annuléé
    public class BookingRejectedConsumer : IConsumer<BookingRejected>
    {
        // Pour chercher et mettre à jour booking en bd
        private readonly IBookingRepository _repository;

        // Pour valider les changements via SaveChangesAsync
        private readonly BookingDbContext _dbContext;

        // pour tracer ce qui passe dans les logs
        private readonly ILogger<BookingRejectedConsumer> _logger;

        public BookingRejectedConsumer(
            IBookingRepository repository,
            BookingDbContext dbContext,
            ILogger<BookingRejectedConsumer> logger)
        {
            _repository = repository;
            _dbContext = dbContext;
            _logger = logger;
        }

        //Consume :c'est la méthode exécutée automatiquement par MassTransit dès qu'un message de type BookingRejected arrive
        public async Task Consume(ConsumeContext<BookingRejected> context)
        {
            // Récuperation du message
            var message = context.Message;

            // Recherche de la réservation 
            var booking = await _repository.GetBySagaCorrelationIdAsync(message.CorrelationId, context.CancellationToken);

            //Gestion des erreurs (Sécurité) si aucune réservation n'est trouvée, elle écrit un avertissement (LogWarning) dans les logs et s'arrête
            // (return;) pour éviter un crash.
            if (booking is null)
            {
                _logger.LogWarning(
                    "Booking introuvable pour SagaCorrelationId {CorrelationId}",
                    message.CorrelationId);
                return;
            }
             // Reject booking
            booking.Reject(message.Reason);

            //prépare la mise à jour en mémoire
            await _repository.UpdateAsync(booking, context.CancellationToken);

            //valide et envoie l'écriture définitive dans la base de données (c'est l'étape où la modification est réellement sauvée).

            await _dbContext.SaveChangesAsync(context.CancellationToken);


            //Confirmation finale : Elle écrit un message de succès dans les logs pour indiquer que tout s'est bien passé.

            _logger.LogWarning(
                "Booking {BookingId} rejeté : {Reason}",
                booking.Id, message.Reason);
        }
    }
}
