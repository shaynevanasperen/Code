using System;
using Code.Extensions.Configuration;
using Code.Extensions.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;

namespace Code.Diagnostics
{
	static class CurrentProcess
	{
		/// <summary>
		/// <para>
		/// Looks for a JSON file with the specified <paramref name="path"/> and if found, an <see cref="IConfiguration"/> is
		/// built from it and compared with another <see cref="IConfiguration"/> built from the current environment variables.
		/// Then for each item in the JSON data which doesn't exist in the environment data, the corresponding environment variable
		/// is set for the current process (using "__" as path separator). When setting an environment variable, the value is
		/// "rendered" using the supplied JSON and environment data to resolve the value of any replacement tokens found.
		/// See documentation on <b>Code.Extensions.Generic.RenderValue</b> for details on rendering.
		/// </para>
		/// <para>
		/// This will not overwrite any environment variables which already exist. It will only set those defined in the file but which
		/// don't exist on the current environment. This means you could publish the file with your app and have the deployment target
		/// host's environment variables take precedence over the ones in the file. However to save on processing, you can decide to not
		/// publish the file if you know your deployment target host already has all the required environment variables configured.
		/// </para>
		/// <para>
		/// This is useful for development, where a machine doesn't or can't have environment variables set, for whatever reason. Having
		/// the ability to set these in a JSON file makes it easier to manipulate during development. If there is no file found at the given
		/// path, this method does nothing, so you <em>can</em> and perhaps <em>should</em> configure your build to not publish this file.
		/// </para>
		/// </summary>
		/// <param name="path">Path relative to the base path.</param>
		/// <param name="fileProvider">The <see cref="IFileProvider"/> to use (defaults to a <see cref="PhysicalFileProvider"/>
		/// with root as the current directory).</param>
		internal static void SubstituteAbsentEnvironmentVariablesFromJsonFile(string path, IFileProvider fileProvider = null)
		{
			if (string.IsNullOrEmpty(path)) throw new ArgumentException("Path cannot be null or empty", nameof(path));

			if (fileProvider == null)
			{
				using var physicalFileProvider = new PhysicalFileProvider(Environment.CurrentDirectory);
				SubstituteAbsentEnvironmentVariablesFromJsonFile(path, physicalFileProvider);
				return;
			}

			if (!fileProvider.GetFileInfo(path).Exists)
				return;

			var substituteData = new ConfigurationBuilder()
				.SetFileProvider(fileProvider)
				.AddJsonFile(path)
				.Build()
				.ToDictionary();

			substituteData.SubstituteAbsentEnvironmentVariablesForCurrentProcess();
		}
	}
}
