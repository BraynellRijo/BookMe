using AutoMapper;
using Domain.Entities.Listings;
using Application.DTOs.ListingDTOs;
using Application.DTOs.PhotoDTOs;

namespace Application.Profiles
{
    public class ListingProfile : Profile
    {
        public ListingProfile()
        {
            CreateMap<ListingCreationDTO, Listing>();
            CreateMap<Listing, ListingDTO>()
                .ForMember(dest => dest.AverageRating, opt => opt.MapFrom(src => src.Reviews != null && src.Reviews.Any() ? Math.Round(src.Reviews.Average(r => (decimal)r.Rating), 1) : 0))
                .ForMember(dest => dest.ReviewsCount, opt => opt.MapFrom(src => src.Reviews != null ? src.Reviews.Count : 0));
            CreateMap<ListingImage, PhotoUploadResultDTO>();

            CreateMap<ListingUpdateDTO, Listing>()
            .ForMember(dest => dest.Capacity, opt => opt.MapFrom(src => src))
            .ForMember(dest => dest.Location, opt => opt.MapFrom(src => src))
            .ForMember(dest => dest.PricingRules, opt => opt.MapFrom(src => src));
            CreateMap<ListingUpdateDTO, ListingCapacity>();
            CreateMap<ListingUpdateDTO, ListingLocation>();
            CreateMap<ListingUpdateDTO, ListingPricingRules>();
        }
    }
}
