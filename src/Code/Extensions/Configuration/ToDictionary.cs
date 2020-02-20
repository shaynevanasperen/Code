using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;

namespace Code.Extensions.Configuration
{
	static partial class ConfigurationProviderExtension
	{
		/// <summary>
		/// Builds a flat dictionary of the key/value pairs represented by the given <see cref="IConfigurationProvider"/> instance.
		/// </summary>
		/// <param name="provider">The <see cref="IConfigurationProvider"/> instance from which to construct the dictionary.</param>
		/// <returns>A flat dictionary of the key/value pairs in the given <see cref="IConfigurationProvider"/> instance.</returns>
		internal static IDictionary<string, string> ToDictionary(this IConfigurationProvider provider)
		{
			if (provider == null) throw new ArgumentNullException(nameof(provider));

			using (var config = new ConfigurationRoot(new[] { provider }))
				return config.ToDictionary();
		}
	}

	static partial class ConfigurationExtension
	{
		/// <summary>
		/// Builds a flat dictionary of the key/value pairs represented by the given <see cref="IConfiguration"/> instance.
		/// </summary>
		/// <param name="configuration">The <see cref="IConfiguration"/> instance from which to construct the dictionary.</param>
		/// <returns>A flat dictionary of the key/value pairs in the given <see cref="IConfiguration"/> instance.</returns>
		internal static IDictionary<string, string> ToDictionary(this IConfiguration configuration)
		{
			if (configuration == null) throw new ArgumentNullException(nameof(configuration));

			var config = configuration.AsEnumerable().Reverse().Where(x => x.Value != null);
			return config.ToDictionary(x => x.Key, x => x.Value, StringComparer.OrdinalIgnoreCase);
		}
	}
}
