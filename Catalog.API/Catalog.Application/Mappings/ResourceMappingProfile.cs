using AutoMapper;
using Catalog.Application.DTOs;
using Catalog.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Catalog.Application.Mappings
{
    // Ce profil AutoMapper définit le dictionnaire de traduction officiel de l'application.
    // Il explique comment transformer automatiquement l'entité technique 'Resource' (base de données)
    // en un enregistrement immuable 'ResourceDto' destiné à l'affichage écran.
    public class ResourceMappingProfile : Profile ////une classe de base obligatoire fournie par la bibliothèque AutoMapper
    {
        public ResourceMappingProfile()
        {
            CreateMap<Resource, ResourceDto>()
               //ForCtorParam  indique à AutoMapper comment mapper une propriété de l'entité source vers un paramètre spécifique du constructeur d'un record immuable.
               .ForCtorParam("TypeLabel", o => o.MapFrom(s => s.Type.ToString()))
               .ForCtorParam("Building", o => o.MapFrom(s => s.Location.Building))
               .ForCtorParam("Floor", o => o.MapFrom(s => s.Location.Floor))
               .ForCtorParam("RoomNumber", o => o.MapFrom(s => s.Location.RoomNumber))
               .ForCtorParam("FullLocation", o => o.MapFrom(s => s.Location.ToString()));
        }
    }
}
