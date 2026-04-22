using Application.DTOs.AuthDTOs;
using AutoMapper;
using Domain.Entities.Users;

namespace Application.Profiles
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            CreateMap<User, LoginDTO>().ReverseMap();

            CreateMap<RegisterDTO, User>()
                .ForMember(dest => dest.Id, opt => opt.Ignore());
        }
    }
}
