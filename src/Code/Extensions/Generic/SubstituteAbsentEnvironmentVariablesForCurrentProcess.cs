using System;
using System.Collections.Generic;
using System.Linq;
using Code.Extensions.Configuration;
using Microsoft.Extensions.Configuration;

namespace Code.Extensions.Generic;

static partial class DictionaryExtension
{
	/// <summary>
	/// An <see cref="IConfiguration"/> is built from the current environment variables and then for each item in
	/// <paramref name="substituteData"/> which doesn't exist in the environment data, the corresponding environment variable
	/// is set for the current process (using "__" as path separator). When setting an environment variable, the value is
	/// "rendered" using the supplied <paramref name="substituteData"/> and environment data to resolve the value of any
	/// replacement tokens found. See documentation on Code.Extensions.Generic.RenderValue for details on rendering.
	/// </summary>
	/// <param name="substituteData">The data to use for substituting absent environment variables.</param>
	internal static void SubstituteAbsentEnvironmentVariablesForCurrentProcess(this IDictionary<string, string?> substituteData)
	{
		if (substituteData == null) throw new ArgumentNullException(nameof(substituteData));

		var environmentData = new ConfigurationBuilder()
			.AddEnvironmentVariables()
			.Build()
			.ToDictionary();

		var allData = new ConfigurationBuilder()
			.AddInMemoryCollection(substituteData)
			.AddInMemoryCollection(environmentData)
			.Build()
			.ToDictionary();

		foreach (var substitute in substituteData.Where(item => !environmentData.ContainsKey(item.Key)))
		{
			// According to https://docs.microsoft.com/en-gb/aspnet/core/fundamentals/configuration/?view=aspnetcore-2.2#environment-variables-configuration-provider
#pragma warning disable CA1307 // Specify StringComparison
			var variable = substitute.Key.Replace(ConfigurationPath.KeyDelimiter, "__");
#pragma warning restore CA1307 // Specify StringComparison
			Environment.SetEnvironmentVariable(variable, substitute.RenderValue(allData));
		}
	}
}