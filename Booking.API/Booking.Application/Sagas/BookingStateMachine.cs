using Booking.Application.Sagas.Events;
using MassTransit;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Booking.Application.Sagas
{
    public  class BookingStateMachine : MassTransitStateMachine<BookingState>
    {
        public State Pending { get; private set; } = null!;
        public State Confirmed { get; private set; } = null!;
        public State Rejected { get; private set; } = null!;

        public Event<BookingRequested> BookingRequestedEvent { get; private set; } = null!;
        public Event<BookingConfirmed> BookingConfirmedEvent { get; private set; } = null!;
        public Event<BookingRejected> BookingRejectedEvent { get; private set; } = null!;


        //constructeur BookingStateMachine c'est une machine à états
        public BookingStateMachine(ILogger<BookingStateMachine> logger)
        {
            // Indique à MassTransit que le champ 'CurrentState' en base de données stockera l'état actuel de la Saga.ççç
            InstanceState(x => x.CurrentState);


            // Si le CorrelationId du message n'existe pas dans la table BookingState, MassTransit créera une nouvelle ligne en base de données.
            // x.CorrelateById : appartient  a BookingState
            Event(() => BookingRequestedEvent,
            x => x.CorrelateById(ctx => ctx.Message.CorrelationId));

            Event(() => BookingConfirmedEvent,
            x => x.CorrelateById(ctx => ctx.Message.CorrelationId));

            Event(() => BookingRejectedEvent,
            x => x.CorrelateById(ctx => ctx.Message.CorrelationId));


            // Bloc de création Initially : s'exécute uniquement si la réservation n'existe pas encore en base de données.
            // 1.Evénement déclencheur   When(BookingRequestedEvent) : réception du message de demande de réservation à l'instant T.
            // 2. Remplissage : MassTransit crée l'objet en RAM (ctx.Saga) et le processeur y copie les données du message (ctx.Message).
            // 3 Traceage logger.LogInformation : journalisation de l'événement de création de la réservation.
            // 3.L'enregistrement final  TransitionTo(Pending)): passage à l'état 'Pending', ce qui déclenche automatiquement l'insertion (INSERT) de la nouvelle ligne en base de données.


            Initially(
             When(BookingRequestedEvent)
                 // .Then(ctx ... La copie des données
                 .Then(ctx =>
                 {
                     ctx.Saga.ResourceId = ctx.Message.ResourceId;
                     ctx.Saga.UserId = ctx.Message.UserId;
                     ctx.Saga.StartTime = ctx.Message.StartTime;
                     ctx.Saga.EndTime = ctx.Message.EndTime;

                     logger.LogInformation(
                         "[Saga] Booking {CorrelationId} démarré pour Resource {ResourceId}",
                         ctx.Saga.CorrelationId, ctx.Saga.ResourceId);
                 })
                 .TransitionTo(Pending));

            //During () Bloc de mise à jour (UPDATE) : la ligne existe obligatoirement en base de données et est à l'état 'Pending'.
            // Selon l'événement ici soit  BookingConfirmedEvent ou bien BookingRejectedEventreçu à l'instant T, le processeur prend l'une des deux trajectoires suivantes :


            During(Pending,
              When(BookingConfirmedEvent)
               .Then(ctx =>
               {
                   ctx.Saga.ResourceName = ctx.Message.ResourceName;

                   logger.LogInformation(
                       "[Saga] Booking {CorrelationId} confirmé — Ressource : {ResourceName}",
                       ctx.Saga.CorrelationId, ctx.Saga.ResourceName);
               })
               .TransitionTo(Confirmed)
               .Finalize(), // Marque ce cycle comme terminé avec échec


             When(BookingRejectedEvent)
               .Then(ctx =>
                {
                   ctx.Saga.RejectionReason = ctx.Message.Reason;

                   logger.LogWarning(
                       "[Saga] Booking {CorrelationId} rejeté — Raison : {Reason}",
                       ctx.Saga.CorrelationId, ctx.Saga.RejectionReason);
                })
               .TransitionTo(Rejected)
               .Finalize());

            SetCompletedWhenFinalized();

        }


    }
}
