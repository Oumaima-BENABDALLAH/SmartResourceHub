using AutoMapper;
using Catalog.Application.DTOs;
using Catalog.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Catalog.Application.Mappings
{
    public class ResourceMappingProfile : Profile
    {
        public ResourceMappingProfile()
        {
            CreateMap<Resource, ResourceDto>()
                .ForMember(d => d.TypeLabel, o => o.MapFrom(s => s.Type.ToString()))
                .ForMember(d => d.Building, o => o.MapFrom(s => s.Location.Building))
                .ForMember(d => d.Floor, o => o.MapFrom(s => s.Location.Floor))
                .ForMember(d => d.RoomNumber, o => o.MapFrom(s => s.Location.RoomNumber))
                .ForMember(d => d.FullLocation, o => o.MapFrom(s => s.Location.ToString()));
        }
    }
}
