using AutoMapper;
using Booking.Application.DTOs;
using Booking.Domain.Repositories;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Booking.Application.Features.Bookings.Queries.GetBookingsByUser
{
    // Classe de traitement (Handler) qui exécute la logique de lecture  : elle récupère les réservations par UserId  et les convertit en une liste de BookingDto en lecture seule.

    public sealed class GetBookingsByUserHandler : IRequestHandler<GetBookingsByUserQuery, IReadOnlyList<BookingDto>>
    {
        private readonly IBookingRepository _repository;
        private readonly IMapper _mapper;

        public GetBookingsByUserHandler(IBookingRepository repository, IMapper mapper)
        {
            _repository= repository;
            _mapper= mapper;
        }
        public async  Task<IReadOnlyList<BookingDto>> Handle(GetBookingsByUserQuery request, CancellationToken cancellationToken)
        {
            var bookings = await _repository.GetByUserIdAsync(request.UserId, cancellationToken);

            return _mapper.Map<IReadOnlyList<BookingDto>>(bookings);
 
        }
    }
}
