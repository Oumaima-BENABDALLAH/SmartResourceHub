using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace Booking.Application.Features.Bookings.Commands.CreateBooking
{
    // Classe de validation qui vérifie la conformité des données (IDs obligatoires, cohérence des dates) 
    // d'une demande de création de réservation avant qu'elle ne soit transmise au Handler.

    public class CreateBookingValidator : AbstractValidator<CreateBookingCommand>
    {
        public CreateBookingValidator() {
            RuleFor(x => x.ResourceId).NotEmpty();
            RuleFor(x => x.UserId).NotEmpty();

            RuleFor(x => x.StartTime).GreaterThan(DateTime.UtcNow)
            .WithMessage("Start time must be in the future.");

            RuleFor(x => x.EndTime).GreaterThan(x=> x.StartTime)
                       .WithMessage("End  time must be after start time.");
        }
    }
}
