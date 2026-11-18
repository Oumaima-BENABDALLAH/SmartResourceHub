using Booking.Domain.Common;
using Booking.Domain.Enums;
using Booking.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Booking.Domain.Entities
{
    public class Booking : BaseEntity     // ==> Aggregate Root (AR) car elle est l’entité principale qui contient toutes les autres entités et objets de valeur liés à la réservation.
                                          // Elle est responsable de la gestion de l’état et des règles métier de la réservation.
    {
        public Guid ResourceId { get; private set; }
        public Guid UserId { get; private set; }
        public string ResourceName { get; private set; } = string.Empty;  // private set; ==> Seule la classe elle-même a le droit de modifier ce nom.
        public DateTime StartTime { get; private set; }
        public DateTime EndTime { get; private set; }
        public BookingStatus Status { get; private set; }
        public string? RejectionReason { get; private set; }
        public Guid SagaCorrelationId { get; private set; }
        private Booking() { }

        public static Booking Create(Guid resourceId, Guid userId, DateTime startTime, DateTime endTime, Guid sagaCorrelationId)
        {
            if (startTime >= endTime) throw new DomainException("Start time must be before end time.");

            if (startTime < DateTime.UtcNow) throw new DomainException("Cannot book a resource in the past.");

            return new Booking
            {
                Id = Guid.NewGuid(),
                ResourceId = resourceId,
                UserId = userId,
                StartTime = startTime,
                EndTime = endTime,
                Status = BookingStatus.Pending,
                SagaCorrelationId = sagaCorrelationId,
                CreatedAt = DateTime.UtcNow
            };


        }

        public void Confirm(string resourceName)
        {
            if (Status != BookingStatus.Pending) throw new DomainException($"Cannot confirm a booking with status '{Status}'.");

            Status = BookingStatus.Confiremed;
            RejectionReason = resourceName;
            SetUpdatedAt();
        }

        public void Reject(string reason)
        {
            if (Status != BookingStatus.Pending) throw new DomainException($"Cannot confirm a booking with status '{Status}'.");

            Status = BookingStatus.Rejected;
            RejectionReason = reason;
            SetUpdatedAt();
        }

        public void Cancel()
        {
            if (Status == BookingStatus.Cancelled) throw new DomainException("Booking is already cancelled.");

            if (Status == BookingStatus.Rejected) throw new DomainException("Booking is already rejected.");

            if (StartTime <= DateTime.UtcNow)  throw new DomainException("Cannot cancel a booking that has already started.");
          
            Status = BookingStatus.Cancelled;
            SetUpdatedAt();
        }
    }
}
