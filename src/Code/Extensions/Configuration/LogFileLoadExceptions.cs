using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Code.Extensions.Configuration
{
	static partial class ConfigurationBuilderExtension
	{
		/// <summary>
		/// Logs any exceptions that occur when loading configuration files.
		/// </summary>
		/// <param name="builder">The instance of <see cref="IConfigurationBuilder"/> to be manipulated.</param>
		/// <param name="getLogger">A callback for obtaining a logger to use for logging exceptions.</param>
		/// <param name="shouldIgnore">Optional callback for determining whether or not an exception should be ignored (swallowed).
		/// If omitted, exceptions with be thrown after being logged.</param>
		/// <returns>The given <see cref="IConfigurationBuilder"/> for method chaining.</returns>
		internal static IConfigurationBuilder LogFileLoadExceptions(this IConfigurationBuilder builder,
			Func<System.Type, ILogger> getLogger,
			Func<FileLoadExceptionContext, bool> shouldIgnore = null)
		{
			if (builder == null) throw new ArgumentNullException(nameof(builder));
			if (getLogger == null) throw new ArgumentNullException(nameof(getLogger));

			builder.SetFileLoadExceptionHandler(context =>
			{
				getLogger(typeof(ConfigurationBuilderExtension))
					.LogError(context.Exception, "Error loading configuration from {Path}", context.Provider.Source.Path);

				context.Ignore = shouldIgnore?.Invoke(context) == true;
			});

			return builder;
		}
	}
}
