using Application.DTOs.ListingDTOs;
using AutoMapper;
using Domain.Entities.Listings;

namespace Application.Profiles
{
    public class AmenityProfile : Profile
    {
        public AmenityProfile()
        {
            CreateMap<AmenityDTO, Amenity>().ReverseMap();
        }
    }
}
