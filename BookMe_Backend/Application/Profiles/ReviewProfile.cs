using Application.DTOs.ListingDTOs.ReviewDTOs;
using AutoMapper;
using Domain.Entities.Listings;

namespace Application.Profiles
{
    public class ReviewProfile : Profile
    {
        public ReviewProfile()
        {
            CreateMap<ReviewCreationDTO, Review>();
            CreateMap<Review, ReviewDTO>();
        }
    }
}
