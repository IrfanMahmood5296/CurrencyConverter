using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Currency.Application.Helpers
{
    public class FileLogger : ILogger
    {
        private readonly string _categoryName;
        private readonly string _folderPath;
        private static readonly object _lock = new object();

        public FileLogger(string categoryName, string folderPath)
        {
            _categoryName = categoryName;
            _folderPath = folderPath;
        }

        public IDisposable BeginScope<TState>(TState state) => null!;

        public bool IsEnabled(LogLevel logLevel) => logLevel != LogLevel.None;

        public void Log<TState>(LogLevel logLevel, EventId eventId,
            TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel)) return;

            string logFile = Path.Combine(_folderPath, $"Log_{DateTime.Now:yyyyMMdd}.txt");

            if (!Directory.Exists(_folderPath))
            {
                Directory.CreateDirectory(_folderPath);
            }

            string message = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] " +
                             $"[{logLevel}] {_categoryName} - {formatter(state, exception)}";

            if (exception != null)
            {
                message += Environment.NewLine +
                           $"Exception: {exception.GetType().FullName}" + Environment.NewLine +
                           $"Message: {exception.Message}" + Environment.NewLine +
                           $"StackTrace: {exception.StackTrace}";
            }

            lock (_lock)
            {
                File.AppendAllText(logFile, message + Environment.NewLine + new string('-', 80) + Environment.NewLine);
            }
        }
    }
}
