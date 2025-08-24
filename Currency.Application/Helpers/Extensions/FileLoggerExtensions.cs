using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Currency.Application.Helpers.Extensions
{
    public static class FileLoggerExtensions
    {
        public static ILoggingBuilder AddFile(this ILoggingBuilder builder)
        {
            var folderPath = Path.Combine(AppContext.BaseDirectory, "Logs");

            builder.AddProvider(new FileLoggerProvider(folderPath));
            return builder;
        }
    }
}
