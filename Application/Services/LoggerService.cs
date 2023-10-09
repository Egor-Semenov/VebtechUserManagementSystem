using Domain.Interfaces.Services;
using Serilog;

namespace Application.Services
{
    public sealed class LoggerService : ILoggerService
    {
        private readonly ILogger _logger;

        public LoggerService()
        {
            _logger = new LoggerConfiguration()
                .WriteTo.Console() // Логирование в консоль
                .CreateLogger();
        }

        public void LogInformation(string message)
        {
            _logger.Information(message);
        }

        public void LogError(string message)
        {
            _logger.Error(message);
        }
    }
}
