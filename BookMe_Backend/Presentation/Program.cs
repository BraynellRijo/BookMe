using Application;
using Application.DTOs.AuthDTOs;
using Application.DTOs.BookingDTOs;
using Application.Profiles;
using Infrastructure;
using Infrastructure.Middlewares;
using Infrastructure.Persistence.Contexts;
using Infrastructure.Services.NotificationServices;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

//AutoMapper
builder.Services.AddAutoMapper(cfg => { }, typeof(UserProfile).Assembly);

//IMemory
builder.Services.AddMemoryCache();

// Application dependencies
builder.Services.AddApplicationServices(builder.Configuration);

// Infrastructure dependencies
builder.Services.AddInfrastructureServices(builder.Configuration);

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
