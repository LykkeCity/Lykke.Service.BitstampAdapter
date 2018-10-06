using System;
using System.Runtime.CompilerServices;
using Common;
using Common.Log;
using Lykke.Common.Log;

namespace Lykke.Service.BitstampAdapter.Extensions
{
   public static class LogExtensions
    {
        public static void ErrorWithDetails(this ILog log,
            Exception exception,
            object context,
            string prefix = "data",
            [CallerMemberName] string process = nameof(ErrorWithDetails))
        {
            log.Error(exception, context: GetContext(context, prefix), process: process);
        }
        
        private static string GetContext(object context, string prefix)
        {
            if (context is string)
                return (string) context;

            return $"{prefix}: {context.ToJson()}";
        }
    }
}
