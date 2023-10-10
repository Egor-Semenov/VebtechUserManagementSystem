using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;

namespace Application.Middleware
{
    public sealed class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandlingMiddleware> _logger;

        public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            HttpStatusCode statusCode;
            string message;

            switch (exception)
            {
                case BadRequestException badRequestException:
                    statusCode = HttpStatusCode.BadRequest;
                    message = badRequestException.Message;
                    break;
                case UnauthorizedException unauthorizedException:
                    statusCode = HttpStatusCode.Unauthorized;
                    message = unauthorizedException.Message;
                    break;
                case NotFoundException notFoundException:
                    statusCode = HttpStatusCode.NotFound;
                    message = notFoundException.Message;
                    break;
                default:
                    statusCode = HttpStatusCode.InternalServerError;
                    message = "An internal server error occurred.";
                    break;
            }

            _logger.LogError($"Error: {message}");

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)statusCode;

            //var errorResponse = new ErrorResponse { Message = message };
            var jsonError = JsonSerializer.Serialize(message);

            await context.Response.WriteAsync(jsonError);
        }
    }

    public sealed class ErrorResponse
    {
        public string Message { get; set; }
    }

    public sealed class BadRequestException : Exception
    {
        public BadRequestException(string message) : base(message) { }
    }

    public sealed class UnauthorizedException : Exception
    {
        public UnauthorizedException(string message) : base(message) { }
    }

    public sealed class NotFoundException : Exception
    {
        public NotFoundException(string message) : base(message) { }
    }
}
