using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Middlewares
{
    public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger = logger;

        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, 
            Exception exception, 
            CancellationToken cancellationToken)
        {
            _logger.LogError(exception, $"Exception occurred: {exception.Message}");

            var problemDetail = new ProblemDetails
            {
                Instance = httpContext.Request.Path
            };

            switch (exception) 
            {
                case ValidationException validationException:
                    httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                    problemDetail.Title = "Validation Error";
                    problemDetail.Detail = "One or more validation errors occurred.";

                    var errors = validationException.Errors
                        .GroupBy(e => e.PropertyName)
                        .ToDictionary(
                            g => g.Key,
                            g => g.Select(e => e.ErrorMessage).ToArray()
                        );
                    problemDetail.Extensions.Add("errors", errors);
                    break;

                case UnauthorizedAccessException unauthorizedAccessException:
                    httpContext.Response.StatusCode = StatusCodes.Status403Forbidden;
                    problemDetail.Title = "Unauthorized";
                    problemDetail.Detail = unauthorizedAccessException.Message;
                    break;

                case KeyNotFoundException keyNotFoundException:
                    httpContext.Response.StatusCode = StatusCodes.Status404NotFound;
                    problemDetail.Title = "Resource Not Found";
                    problemDetail.Detail = keyNotFoundException.Message;
                    break;

                default:
                    httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
                    problemDetail.Title = "Internal Server Error";
                    problemDetail.Detail = "An unexpected error occurred.";
                    break;
            }

            httpContext.Response.StatusCode = problemDetail.Status.Value;
            await httpContext.Response.WriteAsJsonAsync(problemDetail, cancellationToken);

            return true;
        }
    }
}
