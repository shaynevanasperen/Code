using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Code.Threading.Tasks;

static partial class TaskExtension
{
	const string ReasonFormat = "Timed out after {0} seconds.";

	internal static async Task TimeoutAfterSeconds(this Task task, byte seconds, string reasonFormat = ReasonFormat)
	{
		var delay = Debugger.IsAttached
			? TimeSpan.FromMilliseconds(-1)
			: TimeSpan.FromSeconds(seconds);

		if (await Task.WhenAny(task, Task.Delay(delay)).ConfigureAwait(false) == task)
			return;

#pragma warning disable CA1305 // Specify IFormatProvider
		throw new TimeoutException(string.Format(reasonFormat, seconds));
#pragma warning restore CA1305 // Specify IFormatProvider
	}

	internal static async Task<T> TimeoutAfterSeconds<T>(this Task<T> task, byte seconds, string reasonFormat = ReasonFormat)
	{
		var delay = Debugger.IsAttached
			? TimeSpan.FromMilliseconds(-1)
			: TimeSpan.FromSeconds(seconds);

		if (await Task.WhenAny(task, Task.Delay(delay)).ConfigureAwait(false) == task)
			return task.Result;

#pragma warning disable CA1305 // Specify IFormatProvider
		throw new TimeoutException(string.Format(reasonFormat, seconds));
#pragma warning restore CA1305 // Specify IFormatProvider
	}
}