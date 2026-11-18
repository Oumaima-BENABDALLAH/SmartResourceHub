using Booking.Application.Common.Interfaces;
using Booking.Domain.Exceptions;
using Booking.Domain.Repositories;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Booking.Application.Features.Bookings.Commands.CancelBooking
{
    // Classe de traitement (Handler) qui exécute la logique métier  elle récupère la réservation, l'annule et la sauvegarde.
    public class CancelBookingHandler : IRequestHandler<CancelBookingCommand>
    {
        private readonly IBookingRepository _repository;
        private readonly IUnitOfWork _unitOfWork;

        public CancelBookingHandler(IBookingRepository repository, IUnitOfWork unitOfWork)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
        }

        public async  Task Handle(CancelBookingCommand request, CancellationToken cancellationToken)
        {
            var booking = await _repository.GetByIdAsync(request.Id, cancellationToken)
                  ?? throw new BookingNotFoundException(request.Id);

            booking.Cancel();

            await _repository.UpdateAsync(booking, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
