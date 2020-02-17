using System;
using System.Linq;
using Microsoft.Extensions.Configuration;

namespace Code.Extensions.Configuration
{
	static partial class ConfigurationProviderExtension
	{
		/// <summary>
		/// Gets the distinct section names (top-level keys) for which the given <see cref="IConfigurationProvider"/> provides configuration data.
		/// </summary>
		/// <param name="provider">The <see cref="IConfigurationProvider"/> instance from which to get section names.</param>
		/// <returns>An array of the section names (top-level keys) for which the given <see cref="IConfigurationProvider"/>
		/// provides configuration.</returns>
		internal static string[] GetSectionNames(this IConfigurationProvider provider)
		{
			if (provider == null) throw new ArgumentNullException(nameof(provider));

			return provider.GetChildKeys(Enumerable.Empty<string>(), null).Distinct().ToArray();
		}
	}
}
