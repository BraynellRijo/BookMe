using Application.DTOs.NotificationDTOs;
using AutoMapper;
using Domain.Entities.Notifications;

namespace Application.Profiles
{
    public class NotificationProfile : Profile
    {
        public NotificationProfile()
        {
            CreateMap<Notification, NotificationDTO>().ReverseMap();
            CreateMap<NotificationCreationDTO, Notification>().ReverseMap();
        }
    }
}
