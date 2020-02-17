using System;
using Microsoft.Extensions.Configuration;

namespace Code.Extensions.Configuration
{
	static partial class ConfigurationExtension
	{
		/// <summary>
		/// Attempts to bind the configuration section with the given <paramref name="key"/> to a new instance of type <typeparamref name="T"/>.
		/// If that configuration section has a value, that will be used.
		/// Otherwise binding by matching property names against configuration keys recursively.
		/// </summary>
		/// <typeparam name="T">The type of the new instance to bind.</typeparam>
		/// <param name="configuration">The configuration instance to bind.</param>
		/// <param name="key">The key of the configuration section.</param>
		/// <returns>The new instance of <typeparamref name="T"/> if successful, default(<typeparamref name="T"/>) otherwise.</returns>
		internal static T Get<T>(this IConfiguration configuration, string key)
		{
			if (configuration == null) throw new ArgumentNullException(nameof(configuration));
			if (key == null) throw new ArgumentNullException(nameof(key));

			return configuration.GetSection(key).Get<T>();
		}

		/// <summary>
		/// Attempts to bind the configuration section with the given <paramref name="key"/> to a new instance of type <typeparamref name="T"/>.
		/// If that configuration section has a value, that will be used.
		/// Otherwise binding by matching property names against configuration keys recursively.
		/// </summary>
		/// <typeparam name="T">The type of the new instance to bind.</typeparam>
		/// <param name="configuration">The configuration instance to bind.</param>
		/// <param name="key">The key of the configuration section.</param>
		/// <param name="configureOptions">Configures the binder options.</param>
		/// <returns>The new instance of <typeparamref name="T"/> if successful, default(<typeparamref name="T"/>) otherwise.</returns>
		internal static T Get<T>(this IConfiguration configuration, string key, Action<BinderOptions> configureOptions)
		{
			if (configuration == null) throw new ArgumentNullException(nameof(configuration));
			if (key == null) throw new ArgumentNullException(nameof(key));

			return configuration.GetSection(key).Get<T>(configureOptions);
		}
	}
}
