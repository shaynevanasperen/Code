using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;

namespace Code.Extensions.Configuration;

static partial class ConfigurationRootExtension
{
	/// <summary>
	/// Writes log entries when changes are detected, showing which provider triggered the change, which keys were affected,
	/// and optionally the current configuration encompassed by those keys expressed as a dictionary.
	/// </summary>
	/// <param name="configuration">The <see cref="IConfigurationRoot"/> to monitor for changes.</param>
	/// <param name="getLogger">A callback for obtaining a logger to use for logging detected changes.</param>
	/// <param name="logLevel">The <see cref="LogLevel"/> to use (defaults to <see cref="LogLevel.Information"/>).</param>
	/// <param name="withValues">Whether or not to include the full configuration as a nested dictionary (defaults to <c>false</c>).
	/// If <c>false</c>, just the names of the top-level "sections" are included. Beware that when specifying <c>true</c>
	/// here, you may be leaking sensitive information into your logs!</param>
	/// <returns>The given <see cref="IConfigurationRoot"/> for method chaining.</returns>
	internal static IConfigurationRoot LogChanges(this IConfigurationRoot configuration,
		Func<System.Type, ILogger> getLogger,
		LogLevel logLevel = LogLevel.Information,
		bool withValues = false)
	{
		if (configuration == null) throw new ArgumentNullException(nameof(configuration));

		foreach (var provider in configuration.Providers)
		{
			ChangeToken.OnChange(() => provider.GetReloadToken(), state =>
			{
				var logger = getLogger(typeof(ConfigurationRootExtension));
				var scope = withValues
					? new("ProviderConfiguration", state.ToNestedDictionary())
					: new KeyValuePair<string, object>("ProviderSectionNames", state.GetSectionNames());

				using (logger.BeginScope(new[] { scope }))
					logger.Log(logLevel, "Configuration was changed");

			}, provider);
		}

		return configuration;
	}
}