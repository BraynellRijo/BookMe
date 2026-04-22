using Application.DTOs.AuthDTOs;
using Application.DTOs.BookingDTOs;
using Application.Interfaces.Repositories.Bookings;
using Application.Interfaces.Repositories.Listings;
using Application.Interfaces.Repositories.Listings.Amenities;
using Application.Interfaces.Repositories.Listings.ListingBlocks;
using Application.Interfaces.Repositories.Listings.ListingImages;
using Application.Interfaces.Repositories.Listings.Reviews;
using Application.Interfaces.Repositories.Notifications;
using Application.Interfaces.Repositories.Users;
using Application.Interfaces.Services.AuthServices;
using Application.Interfaces.Services.BookingServices;
using Application.Interfaces.Services.EmailServices;
using Application.Interfaces.Services.ListingServices;
using Application.Interfaces.Services.NotificationServices;
using Application.Interfaces.Services.PhotoServices;
using Application.Profiles;
using Application.Services.AuthService;
using Application.Services.BookingService;
using Application.Services.ListingService;
using Application.Services.NotificationService;
using Domain.Entities.Users;
using FluentValidation;
using Infrastructure.Middlewares;
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
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

//Connection to DB
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("BookMeDBConnection"),
        sqlOptions => sqlOptions.UseNetTopologySuite()
    )
);
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

//AutoMapper
builder.Services.AddAutoMapper(cfg => { }, typeof(UserProfile).Assembly);
//IMemory
builder.Services.AddMemoryCache();
//SignalR
builder.Services.AddSignalR();

//Repositories - Internals
builder.Services.AddScoped<IQueryUserRepository, UserRepository>();
builder.Services.AddScoped<ICommandUserRepository, UserRepository>();
//Listing Repository
builder.Services.AddScoped<IQueryListingRepository, ListingRepository>();
builder.Services.AddScoped<IFilterListingRepository, ListingRepository>();
builder.Services.AddScoped<ICommandListingRepository, ListingRepository>();
//ListingImage Repository
builder.Services.AddScoped<IQueryListingImagesRepository, ListingImagesRepository>();
builder.Services.AddScoped<ICommandListingImagesRepository, ListingImagesRepository>();
//Listing Block Repository
builder.Services.AddScoped<IQueryListingBlockRepository, ListingBlockRepository>();
builder.Services.AddScoped<ICommandListingBlockRepository, ListingBlockRepository>();
//Review Repository
builder.Services.AddScoped<IQueryReviewRepository, ReviewRepository>();
builder.Services.AddScoped<ICommandReviewRepository, ReviewRepository>();
//Amenity Repository
builder.Services.AddScoped<IQueryAmenityRepository, AmenityRepository>();
builder.Services.AddScoped<ICommandAmenityRepository, AmenityRepository>();

//Booking Repositoriy
builder.Services.AddScoped<IQueryBookingRepository, BookingRepository>();
builder.Services.AddScoped<ICommandBookingRepository, BookingRepository>();
//Notification Repository
builder.Services.AddScoped<IQueryNotificationRepository, NotificationRepository>();
builder.Services.AddScoped<ICommandNotificationRepository, NotificationRepository>();

//Validators - FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<BookingCreationDTO>();
builder.Services.AddValidatorsFromAssemblyContaining<BookingDTO>();
builder.Services.AddValidatorsFromAssemblyContaining<RegisterDTO>();
builder.Services.AddValidatorsFromAssemblyContaining<LoginDTO>();

//Services - Infraestructure
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
builder.Services.AddScoped<IPhotoService, PhotoService>();
builder.Services.AddScoped<IEmailServices, EmailServices>();
builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("SmtpSettings"));
builder.Services.Configure<CloudinarySettings>(builder.Configuration.GetSection("CloudinarySettings"));
builder.Services.AddScoped<IRealTimeNotificationService, SignalRNotificationService>();

//Services - Application
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IListingService, ListingService>();
builder.Services.AddScoped<IHostListingService, HostListingService>();
//builder.Services.AddScoped<IGuestListingService, GuestListingService>();
builder.Services.AddScoped<IReviewService, ReviewService>();
builder.Services.AddScoped<IGuestBookingService, GuestBookingService>();
builder.Services.AddScoped<IHostBookingService, HostBookingService>();
builder.Services.AddScoped<IAvailabilityService, AvailabilityService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IAmenityService, AmenityService>();

//JWT
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["jwt:Audience"],
            ValidateLifetime = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["jwt:Key"]!)),
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
builder.Services.AddHostedService<BookingCompletionWorker>();

//Cors
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBookMeFront",
        builder =>
        {
            builder.WithOrigins("http://localhost:4200")
                   .AllowAnyMethod()
                   .AllowAnyHeader()
                   .AllowCredentials();
        });
});

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();
app.UseExceptionHandler();

app.UseCors("AllowBookMeFront");
app.UseAuthentication();
app.UseAuthorization();
app.MapHub<NotificationHub>("/hubs/notifications"); 
app.MapControllers();

app.Run();
