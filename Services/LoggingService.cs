using Scribe.Data;
using Scribe.Infrastructure;
using Scribe.Models;

namespace Scribe.Services
{
    public interface ILoggingService
    {
        Task LogActionAsync(string details, string user);
    }
    public class LoggingService: ILoggingService
    {
        private readonly ApplicationDbContext _context;
        public LoggingService(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task LogActionAsync(string details, string user)
        {
            var logEntry = new Log
            {
                Details = details,
                User = user,
                Date = DateTime.UtcNow
            };

            _context.Log.Add(logEntry);
            await _context.SaveChangesAsync(); // Save to database
        }
    }
}
