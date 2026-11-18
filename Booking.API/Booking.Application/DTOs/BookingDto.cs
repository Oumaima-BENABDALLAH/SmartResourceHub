using Booking.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Booking.Application.DTOs
{
    // Classe de transfert (DTO) non héritable(sealed) et immuable (non modifiable record), servant uniquement à renvoyer les données de la réservation au client HTTP.
    public sealed  record BookingDto
     (Guid Id,
    Guid ResourceId,
    Guid UserId,
    string ResourceName,
    DateTime StartTime,
    DateTime EndTime,
    BookingStatus Status,
    string StatusLabel,
    string? RejectionReason,
    Guid SagaCorrelationId,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
}
