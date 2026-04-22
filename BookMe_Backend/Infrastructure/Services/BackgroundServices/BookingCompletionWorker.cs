using Application.Interfaces.Services.BookingServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services.BackgroundServices
{
    public class BookingCompletionWorker(
        ILogger<BookingCompletionWorker> logger,
        IServiceProvider serviceProvider) : BackgroundService
    {
            private readonly ILogger<BookingCompletionWorker> _logger = logger;
            private readonly IServiceProvider _serviceProvider = serviceProvider;
        
        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Booking Completion Worker is starting.");
            using var timer = new PeriodicTimer(TimeSpan.FromDays(1));

            try
            {
                while (await timer.WaitForNextTickAsync(stoppingToken))
                {
                    _logger.LogInformation($"Booking Completion Worker is running at: {DateTime.UtcNow}");

                    try
                    {
                        using var scope = _serviceProvider.CreateScope();
                        var bookingService = scope.ServiceProvider.GetRequiredService<IGuestBookingService>();

                        await bookingService.UpdateStatusCompleted();

                        _logger.LogInformation("Booking Completion Worker is sleeping for 1 day.");

                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "An error occurred while updating booking statuses.");
                    }
                }
                }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Booking Completion Worker is stopping.");
            }
   

        }
    }
}
