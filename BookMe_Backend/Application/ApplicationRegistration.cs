using Application.DTOs.AuthDTOs;
using Application.DTOs.BookingDTOs;
using Application.Interfaces.Services.AuthServices;
using Application.Interfaces.Services.BookingServices;
using Application.Interfaces.Services.ListingServices;
using Application.Interfaces.Services.NotificationServices;
using Application.Services.AuthService;
using Application.Services.BookingService;
using Application.Services.ListingService;
using Application.Services.NotificationService;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using FluentValidation;

namespace Application
{
    public static class ApplicationRegistration
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            //Validators - FluentValidation
            services.AddValidatorsFromAssemblyContaining<BookingCreationDTO>();
            services.AddValidatorsFromAssemblyContaining<BookingDTO>();
            services.AddValidatorsFromAssemblyContaining<RegisterDTO>();
            services.AddValidatorsFromAssemblyContaining<LoginDTO>();

            //Services - Application
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IListingService, ListingService>();
            services.AddScoped<IHostListingService, HostListingService>();
            //services.AddScoped<IGuestListingService, GuestListingService>();
            services.AddScoped<IReviewService, ReviewService>();
            services.AddScoped<IGuestBookingService, GuestBookingService>();
            services.AddScoped<IHostBookingService, HostBookingService>();
            services.AddScoped<IAvailabilityService, AvailabilityService>();
            services.AddScoped<INotificationService, NotificationService>();
            services.AddScoped<IAmenityService, AmenityService>();

            return services;
        }
    }
}
