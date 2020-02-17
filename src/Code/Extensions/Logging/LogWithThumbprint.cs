using System.Collections.Generic;
using Code.Extensions.Exception;
using Microsoft.Extensions.Logging;

namespace Code.Extensions.Logging
{
	static partial class LoggerExtension
	{
		internal const string ThumbprintKey = "ExceptionThumbprint";
		internal const string TypeKey = "ExceptionType";

		/// <summary>
		/// Formats and writes a log message at the log level given by <paramref name="logLevel"/>, adding context properties for easier diagnosis.
		/// </summary>
		/// <param name="logger">The <see cref="ILogger"/> to write to.</param>
		/// <param name="logLevel">The log level to use.</param>
		/// <param name="exception">The exception to log.</param>
		/// <param name="message">Format string of the log message.</param>
		/// <param name="args">An object array that contains zero or more objects to format.</param>
		internal static void LogWithThumbprint(this ILogger logger, LogLevel logLevel, System.Exception exception, string message, params object[] args)
		{
			if (!logger.IsEnabled(logLevel))
				return;

			var thumbprint = exception.GetFingerprint();
			var scope = new Dictionary<string, object>
			{
				{ ThumbprintKey, thumbprint },
				{ TypeKey, exception?.GetType().Name }
			};

			using (logger.BeginScope(scope))
			{
				logger.Log(logLevel, exception, message, args);
			}
		}
	}
}
