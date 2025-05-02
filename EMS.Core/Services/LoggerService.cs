using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EMS.Core.Services
{
    public class LoggerService : ILoggerService, IDisposable
    {
        private static readonly SemaphoreSlim _semaphore = new(1, 1);
        private readonly List<string> _cache = new();
        private readonly string _logDirectory;
        private readonly Timer _flushTimer;

        public LoggerService()
        {
            _logDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");

            if (!Directory.Exists(_logDirectory))
            {
                Directory.CreateDirectory(_logDirectory);
            }

            _flushTimer = new Timer(async _ => await FlushCacheToFileAsync(), null, TimeSpan.Zero, TimeSpan.FromSeconds(10));
        }

        public async Task LogErrorAsync(Exception ex,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0)
        {
            string location = $"{Path.GetFileName(filePath)} -> {memberName}() [Line {lineNumber}]";
            string reason = ex.Message;

            var logEntry = BuildLogEntry(ex, location, reason);

            await _semaphore.WaitAsync();
            try
            {
                _cache.Add(logEntry);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private string BuildLogEntry(Exception ex, string location, string reason)
        {
            var sb = new StringBuilder();
            sb.AppendLine("--------------------------------------------------");
            sb.AppendLine($"Time: {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}");
            sb.AppendLine($"Location: {location}");
            sb.AppendLine($"Reason: {reason}");
            sb.AppendLine($"Exception Type: {ex.GetType().Name}");
            sb.AppendLine($"Full Message: {ex}");
            sb.AppendLine("--------------------------------------------------");
            return sb.ToString();
        }

        private async Task FlushCacheToFileAsync()
        {
            await _semaphore.WaitAsync();
            try
            {
                if (_cache.Count == 0) return;

                string logFile = Path.Combine(_logDirectory, $"{DateTime.Now:yyyy-MM-dd}.log");
                await File.AppendAllLinesAsync(logFile, _cache);
                _cache.Clear();
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public void Dispose()
        {
            _flushTimer?.Dispose();
            FlushCacheToFileAsync().GetAwaiter().GetResult();
        }

    }
}