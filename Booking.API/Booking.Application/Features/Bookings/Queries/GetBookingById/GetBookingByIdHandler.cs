using AutoMapper;
using Booking.Application.Common.Interfaces;
using Booking.Application.DTOs;
using Booking.Domain.Exceptions;
using Booking.Domain.Repositories;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Booking.Application.Features.Bookings.Queries.GetBookingById
{
    // Classe de traitement (Handler) qui exécute la logique de lecture  : elle récupère la réservation  et la convertit en BookingDto.
    public sealed class GetBookingByIdHandler : IRequestHandler<GetBookingByIdQuery, BookingDto>
    {

        private readonly IBookingRepository _bookingRepository;
        private readonly IMapper _mapper;


        public GetBookingByIdHandler(IBookingRepository bookingRepository , IMapper mapper)
        {
            _bookingRepository = bookingRepository;
            _mapper = mapper;
        }

        public async  Task<BookingDto> Handle(GetBookingByIdQuery request, CancellationToken cancellationToken)
        {
            var booking = await _bookingRepository.GetByIdAsync(request.id, cancellationToken)
                  ?? throw new BookingNotFoundException(request.id);


            return _mapper.Map<BookingDto>(booking);
        }
    }
}
