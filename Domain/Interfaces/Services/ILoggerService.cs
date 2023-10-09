
namespace Domain.Interfaces.Services
{
    public interface ILoggerService
    {
        void LogInformation(string message);
        void LogError(string message);
    }
}
