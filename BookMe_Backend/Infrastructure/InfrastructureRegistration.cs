using Application.Interfaces.Repositories.Bookings;
using Application.Interfaces.Repositories.Listings;
using Application.Interfaces.Repositories.Listings.Amenities;
using Application.Interfaces.Repositories.Listings.ListingBlocks;
using Application.Interfaces.Repositories.Listings.ListingImages;
using Application.Interfaces.Repositories.Listings.Reviews;
using Application.Interfaces.Repositories.Notifications;
using Application.Interfaces.Repositories.Users;
using Application.Interfaces.Services.EmailServices;
using Application.Interfaces.Services.NotificationServices;
using Application.Interfaces.Services.PhotoServices;
using Domain.Entities.Users;
using Infrastructure.Persistence.Contexts;
using Infrastructure.Persistence.Repositories;
using Infrastructure.Persistence.Repositories.ListingRepositories;
using Infrastructure.Services.BackgroundServices;
using Infrastructure.Services.NotificationServices;
using Infrastructure.Services.PhotoServices;
using Infrastructure.Services.SmtpEmailService;
using Infrastructure.Settings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Reflection.Metadata.Ecma335;
using System.Text;

namespace Infrastructure
{
    public static class InfrastructureRegistration
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            //Connection to DB
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(
                    configuration.GetConnectionString("BookMeDBConnection"),
                    sqlOptions => sqlOptions.UseNetTopologySuite()
                )
            );


            //Services - Infraestructure
            services.AddHttpContextAccessor();
            services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
            services.AddScoped<IPhotoService, PhotoService>();
            services.AddScoped<IEmailServices, EmailServices>();
            services.Configure<SmtpSettings>(configuration.GetSection("SmtpSettings"));
            services.Configure<CloudinarySettings>(configuration.GetSection("CloudinarySettings"));
            services.AddScoped<IRealTimeNotificationService, SignalRNotificationService>();

            //SignalR
            services.AddSignalR();

            //Repositories - Internals
            services.AddScoped<IQueryUserRepository, UserRepository>();
            services.AddScoped<ICommandUserRepository, UserRepository>();
            //Listing Repository
            services.AddScoped<IQueryListingRepository, ListingRepository>();
            services.AddScoped<IFilterListingRepository, ListingRepository>();
            services.AddScoped<ICommandListingRepository, ListingRepository>();
            //ListingImage Repository
            services.AddScoped<IQueryListingImagesRepository, ListingImagesRepository>();
            services.AddScoped<ICommandListingImagesRepository, ListingImagesRepository>();
            //Listing Block Repository
            services.AddScoped<IQueryListingBlockRepository, ListingBlockRepository>();
            services.AddScoped<ICommandListingBlockRepository, ListingBlockRepository>();
            //Review Repository
            services.AddScoped<IQueryReviewRepository, ReviewRepository>();
            services.AddScoped<ICommandReviewRepository, ReviewRepository>();
            //Amenity Repository
            services.AddScoped<IQueryAmenityRepository, AmenityRepository>();
            services.AddScoped<ICommandAmenityRepository, AmenityRepository>();

            //Booking Repositoriy
            services.AddScoped<IQueryBookingRepository, BookingRepository>();
            services.AddScoped<ICommandBookingRepository, BookingRepository>();
            //Notification Repository
            services.AddScoped<IQueryNotificationRepository, NotificationRepository>();
            services.AddScoped<ICommandNotificationRepository, NotificationRepository>();


            //JWT
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = configuration["jwt:Issuer"],
                        ValidateAudience = true,
                        ValidAudience = configuration["jwt:Audience"],
                        ValidateLifetime = true,
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(configuration["jwt:Key"]!)),
                        ValidateIssuerSigningKey = true
                    };
                    options.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            var accessToken = context.Request.Query["access_token"];
                            var path = context.HttpContext.Request.Path;

                            if (!string.IsNullOrEmpty(accessToken) &&
                                path.StartsWithSegments("/hubs/notifications"))
                            {
                                context.Token = accessToken;
                            }
                            return Task.CompletedTask;
                        }
                    };
                });

            //Background Services
            services.AddHostedService<BookingCompletionWorker>();
        
            return services;
        }
        
    }
}
