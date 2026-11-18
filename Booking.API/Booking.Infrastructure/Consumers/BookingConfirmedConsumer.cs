using Booking.Application.Sagas.Events;
using Booking.Domain.Repositories;
using Booking.Infrastructure.Persistence;
using Microsoft.Extensions.Logging;
using MassTransit;
using System;
using System.Collections.Generic;
using System.Text;

namespace Booking.Infrastructure.Consumers
{

   // un Consumer(consommateur) MassTransit.Son rôle est d'écouter un message asynchrone (un événement) provenant d'un bus de messages(comme RabbitMQ)
   // pour réagir lorsqu'une réservation est confirmée
    public class BookingConfirmedConsumer : IConsumer<BookingConfirmed>
    {

        //IBookingRepository : pour chercher et mettre à jour la réservation en base.
        private readonly IBookingRepository _repository;

        //BookingDbContext pour valider les changements via SaveChangesAsync
        private readonly BookingDbContext _dbContext;

        //ILogger pour tracer ce qui se passe dans les logs (très utile pour le débogage).
        private readonly ILogger<BookingConfirmedConsumer> _logger;

        public BookingConfirmedConsumer(
            IBookingRepository repository,
            BookingDbContext dbContext,
            ILogger<BookingConfirmedConsumer> logger)
        {
            _repository = repository;
            _dbContext = dbContext;
            _logger = logger;
        }

        //Consume :c'est la méthode exécutée automatiquement par MassTransit dès qu'un message de type BookingConfirmed arrive
        public async Task Consume(ConsumeContext<BookingConfirmed> context)
        {
            //Récupération du message
            var message = context.Message;

            //Recherche de la réservation
            var booking = await _repository.GetBySagaCorrelationIdAsync(message.CorrelationId);
            if (booking == null)
            {
                _logger.LogWarning("Booking introuvable pour SagaCorrelationId {CorrelationId}", message.CorrelationId);
                return;
            }

            // Mise à jour statut 
            booking.Confirm(message.ResourceName);

            //prépare la mise à jour en mémoire
            await _repository.UpdateAsync(booking, context.CancellationToken);

            //valide et envoie l'écriture définitive dans la base de données (c'est l'étape où la modification est réellement sauvée).
            await _dbContext.SaveChangesAsync(context.CancellationToken);

            //Confirmation finale : Elle écrit un message de succès dans les logs pour indiquer que tout s'est bien passé.
            _logger.LogInformation(
                "Booking {BookingId} confirmé avec succès (Resource: {ResourceName})",
                booking.Id, message.ResourceName);
        }
    }
}
