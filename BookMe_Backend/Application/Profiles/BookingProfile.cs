using Application.DTOs.BookingDTOs;
using AutoMapper;
using Domain.Entities.Bookings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Application.Profiles
{
    public class BookingProfile : Profile
    {
        public BookingProfile()
        {
            CreateMap<BookingCreationDTO, Booking>();
            
            CreateMap<Booking, BookingDTO>()
                .ForMember(dest => dest.ListingTitle, opt => opt.MapFrom(src => src.Listing != null ? src.Listing.Title : null))
                .ForMember(dest => dest.ListingLocation, opt => opt.MapFrom(src => src.Listing != null ? $"{src.Listing.Location.City}, {src.Listing.Location.Country}" : null))
                .ForMember(dest => dest.ListingImageUrl, opt => opt.MapFrom(src => 
                    src.Listing != null && src.Listing.Images != null && src.Listing.Images.Any() 
                        ? (src.Listing.Images.FirstOrDefault(img => img.Order == 0) ?? src.Listing.Images.First()).Url 
                        : "assets/images/placeholder.png"));

            CreateMap<Booking, BookedDateRangeDTO>();
        }
    }
}
