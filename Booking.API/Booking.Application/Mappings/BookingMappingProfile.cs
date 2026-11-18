using AutoMapper;
using Booking.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace Booking.Application.Mappings
{
    // Ce profil AutoMapper définit le dictionnaire de traduction officiel de l'application.
    // Il explique comment transformer automatiquement l'entité technique 'Booking' (base de données)
    // en un enregistrement immuable 'BookingDto' destiné à l'affichage écran.
    public class BookingMappingProfile : Profile //une classe de base obligatoire fournie par la bibliothèque AutoMapper
    {
        public BookingMappingProfile() {
            CreateMap<Booking.Domain.Entities.Booking, BookingDto>()
                 .ForMember(d => d.StatusLabel, o => o.MapFrom(s => s.Status.ToString()))
                
                 .ConstructUsing(s => new BookingDto(
                     s.Id,
                     s.ResourceId,
                     s.UserId,
                     "Inconnu", 
                     s.StartTime,
                     s.EndTime,
                     s.Status,
                     s.Status.ToString(),
                     s.RejectionReason,
                     s.SagaCorrelationId,
                     s.CreatedAt,
                     s.UpdatedAt
                 ));
        }
    }

}
