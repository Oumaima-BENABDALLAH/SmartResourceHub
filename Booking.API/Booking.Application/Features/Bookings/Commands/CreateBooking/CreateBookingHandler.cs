using AutoMapper;
using Booking.Application.Common.Interfaces;
using Booking.Application.DTOs;
using Booking.Application.Sagas.Events;
using Booking.Domain.Repositories;
using MassTransit;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Booking.Application.Features.Bookings.Commands.CreateBooking
{
    //le classe Handler représente le "Comment" , Le cerveau qui utilise ces données pour faire le travail
    // L'interface IRequestHandler de MediatR fait le lien automatique entre la CreateBookingCommand et ce CreateBookingHandler, 
    // et spécifie que la réponse finale sera un 'BookingDto'.
    public sealed class CreateBookingHandler :IRequestHandler<CreateBookingCommand, BookingDto>

    {
        //IBookingRepository est la passerelle exclusive entre votre logique métier et votre base de données
        private readonly IBookingRepository _repository;

        //_unitOfWork valide l'écriture en Base de Données
        private readonly IUnitOfWork _unitOfWork;

        //_publishEndpoint envoie le signal à Azure Service Bus
        private readonly IPublishEndpoint _publishEndpoint;

        //_mapper prépare le résultat propre (DTO) pour l'afficher à l'écran.
        private readonly IMapper _mapper;

        public CreateBookingHandler(
        IBookingRepository repository,
        IUnitOfWork unitOfWork,
        IPublishEndpoint publishEndpoint,
        IMapper mapper)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
            _publishEndpoint = publishEndpoint;
            _mapper = mapper;
        }


        public async  Task<BookingDto> Handle(CreateBookingCommand request, CancellationToken cancellationToken)
        {
            var confilt = await _repository.HasConflictAsync(request.ResourceId, request.StartTime, request.EndTime, cancellationToken);
            if (confilt) throw new InvalidOperationException("This resource is already booked for the requested time slot.");

            var sagaCorrelationId = Guid.NewGuid();

            var booking = Domain.Entities.Booking.Create(request.ResourceId,
            request.UserId,
            request.StartTime,
            request.EndTime,
            sagaCorrelationId);

            await _repository.AddAsync(booking, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _publishEndpoint.Publish(new BookingRequested
            {
                CorrelationId = sagaCorrelationId,
                ResourceId = request.ResourceId,
                UserId = request.UserId,
                StartTime = request.StartTime,
                EndTime = request.EndTime
            }, cancellationToken);

            return _mapper.Map<BookingDto>(booking);

        }
    }
}
