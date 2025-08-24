using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Currency.Application.Helpers.Extensions
{
    public class FileLoggerProvider : ILoggerProvider
    {
        private readonly string _folderPath;

        public FileLoggerProvider(string folderPath)
        {
            _folderPath = folderPath;
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new FileLogger(categoryName, _folderPath);
        }

        public void Dispose() { }
    }
}
