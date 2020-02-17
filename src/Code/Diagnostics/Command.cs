using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace Code.Diagnostics
{
	enum CommandType
	{
		Standard,
		Terminal,
		Shell
	}

	static class Command
	{
		/// <summary>
		/// Invokes the given <paramref name="command"/> with the given <paramref name="arguments"/>,
		/// using the given <paramref name="timeout"/> and <paramref name="logger"/>.
		/// </summary>
		/// <param name="command">The command to invoke.</param>
		/// <param name="arguments">The arguments to pass to the invocation.</param>
		/// <param name="timeout">The <see cref="TimeSpan"/> for how long to wait for the process to exit.</param>
		/// <param name="logger">A logger for logging output and errors.</param>
		/// <param name="commandType">The type of command.</param>
		/// <param name="hasError">A callback for determining whether there are any errors in the output.</param>
		internal static void Run(
			string command,
			string arguments,
			TimeSpan timeout,
			ILogger logger,
			CommandType commandType = CommandType.Standard,
			Func<string, bool> hasError = null)
		{
			if (command == null) throw new ArgumentNullException(nameof(command));
			if (arguments == null) throw new ArgumentNullException(nameof(arguments));
			if (timeout == TimeSpan.Zero) throw new ArgumentException("Timeout must be non-zero.", nameof(timeout));
			if (logger == null) throw new ArgumentNullException(nameof(logger));

			using var process = StartProcess(command, arguments, commandType, logger, out var output, out var error);

			if (commandType == CommandType.Terminal)
			{
				var exitCode = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "%errorlevel%" : "$?";

				process.StandardInput.WriteLine($"{command} {arguments}");
				process.StandardInput.WriteLine($"exit {exitCode}");
			}

			var milliseconds = Math.Abs((int)timeout.TotalMilliseconds);
			if (process.WaitForExit(milliseconds) &&
				output.WaitHandle.WaitOne(milliseconds) &&
				error.WaitHandle.WaitOne(milliseconds))
			{
				hasError ??= text => false;
				if (commandType != CommandType.Shell && process.ExitCode != 0 ||
					hasError(output.Text.ToString()) ||
					hasError(error.Text.ToString()))
				{
					throw new Exception($"{command} exited with code {process.ExitCode}.{Environment.NewLine}{output.Text}{Environment.NewLine}{error.Text}");
				}
			}
			else
			{
				throw new Exception($"{command} timed out.");
			}
		}

		private static Process StartProcess(string command, string arguments, CommandType commandType, ILogger logger,
			out DataReceiver output, out DataReceiver error)
		{
			var terminalFileName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "cmd.exe" : "bash";
			var useShellExecute = commandType == CommandType.Shell;
			var process = new Process
			{
				StartInfo =
				{
					FileName = commandType == CommandType.Terminal ? terminalFileName : command,
					Arguments = commandType == CommandType.Terminal ? "" : arguments,
					UseShellExecute = useShellExecute,
					LoadUserProfile = useShellExecute,
					WorkingDirectory = Environment.CurrentDirectory,
					RedirectStandardInput = !useShellExecute,
					RedirectStandardOutput = !useShellExecute,
					RedirectStandardError = !useShellExecute
				}
			};

			try
			{
				output = new DataReceiver().Subscribe(x => logger.LogInformation(x));
				process.OutputDataReceived += output.HandleData;

				error = new DataReceiver().Subscribe(x => logger.LogError(x));
				process.ErrorDataReceived += error.HandleData;

				process.Start();
				if (process.StartInfo.RedirectStandardInput)
				{
					process.BeginOutputReadLine();
				}
				else
				{
					output.WaitHandle.Set();
				}
				if (process.StartInfo.RedirectStandardError)
				{
					process.BeginErrorReadLine();
				}
				else
				{
					error.WaitHandle.Set();
				}
				return process;
			}
			catch
			{
				process.Dispose();
				throw;
			}
		}

		private class DataReceiver
		{
			private Action<string> _subscription;

			public void HandleData(object sender, DataReceivedEventArgs args)
			{
				if (args.Data == null)
				{
					WaitHandle.Set();
				}
				else
				{
					_subscription?.Invoke(args.Data);
					Text.AppendLine(args.Data);
				}
			}

			public DataReceiver Subscribe(Action<string> action)
			{
				_subscription = action;
				return this;
			}

			public EventWaitHandle WaitHandle { get; } = new AutoResetEvent(false);
			public StringBuilder Text { get; } = new StringBuilder();
		}
	}
}
