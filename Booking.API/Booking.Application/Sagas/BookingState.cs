using MassTransit;
using System;
using System.Collections.Generic;
using System.Text;

namespace Booking.Application.Sagas
{
    // Représente la mémoire vivante (l'état persistant) de la Saga de réservation.
    // Cette classe stocke toutes les informations d'une transaction et son statut actuel 

    public class BookingState : SagaStateMachineInstance  //un contrat MassTransit obligatoire ,  elle force la classe à avoir un CorrelationId pour identifier de manière unique 
    {
        public Guid CorrelationId { get; set; }


        // Stocke l'état actuel de la Saga ( Submitted, Completed ...)
        // Le '= default!;' évite l'avertissement de valeur nulle du compilateur car MassTransit initialise cette valeur avant toute sauvegarde en base de données.
        public string CurrentState { get; set; } = default!;

        public Guid ResourceId { get; set; }
        public Guid UserId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string ResourceName { get; set; } = string.Empty;
        public string? RejectionReason { get; set; }
    }
}
