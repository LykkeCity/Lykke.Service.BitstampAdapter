using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common.Log;
using Lykke.Common.ApiLibrary.Middleware;
using Lykke.MonitoringServiceApiCaller;
using Lykke.Sdk;
using Lykke.Sdk.Settings;
using Lykke.SettingsReader;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Lykke.Service.BitstampAdapter
{
    public class ErrorResponse
    {
        /// <summary>
        /// Summary error message
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Model errors. Key is the model field name, value is the list of the errors related to the given model field.
        /// </summary>
        public Dictionary<string, List<string>> ModelErrors { get; set; }

        /// <summary>
        /// Creates <see cref="ErrorResponse"/> with summary error message
        /// </summary>
        /// <param name="message">Summary error message</param>
        public static ErrorResponse Create(string message)
        {
            return new ErrorResponse
            {
                ErrorMessage = message,
                ModelErrors = new Dictionary<string, List<string>>()
            };
        }

        /// <summary>
        /// Adds model error to the current <see cref="ErrorResponse"/> instance
        /// </summary>
        /// <param name="key">Model field name</param>
        /// <param name="message">Error related to the given model field</param>
        /// <returns></returns>
        public ErrorResponse AddModelError(string key, string message)
        {
            if (ModelErrors == null)
            {
                ModelErrors = new Dictionary<string, List<string>>();
            }

            if (!ModelErrors.TryGetValue(key, out var errors))
            {
                errors = new List<string>();

                ModelErrors.Add(key, errors);
            }

            errors.Add(message);

            return this;
        }

        /// <summary>
        /// Adds model error to the current <see cref="ErrorResponse"/> instance
        /// </summary>
        /// <param name="key">Model field name</param>
        /// <param name="exception">Exception which corresponds to the error related to the given model field</param>
        public ErrorResponse AddModelError(string key, Exception exception)
        {
            var ex = exception;
            var sb = new StringBuilder();

            while (true)
            {
                if (ex.InnerException != null)
                {
                    sb.AppendLine(ex.Message);
                }
                else
                {
                    sb.Append(ex.Message);
                }

                ex = ex.InnerException;

                if (ex == null)
                {
                    return AddModelError(key, sb.ToString());
                }

                sb.Append(" -> ");
            }
        }

        public string GetSummaryMessage()
        {
            var sb = new StringBuilder();

            if (ErrorMessage != null)
            {
                sb.AppendLine($"Error summary: {ErrorMessage}");
            }

            if (ModelErrors == null)
                return sb.ToString();

            sb.AppendLine();

            foreach (var error in ModelErrors)
            {
                if (error.Key == null || error.Value == null || error.Value.Count == 0)
                    continue;

                if (!string.IsNullOrWhiteSpace(error.Key))
                {
                    sb.AppendLine($"{error.Key}:");
                }

                foreach (var message in error.Value.Take(error.Value.Count - 1))
                {
                    sb.AppendLine($" - {message}");
                }

                sb.Append($" - {error.Value.Last()}");
            }

            return sb.ToString();
        }
    }

    public class LykkeConfigurationOptions
    {
        /// <summary>Default error handler.</summary>
        public CreateErrorResponse DefaultErrorHandler { get; set; }

        internal LykkeConfigurationOptions()
        {
            DefaultErrorHandler = ex => ErrorResponse.Create("Technical problem");
        }
    }

}
