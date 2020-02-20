﻿using System;
using System.Threading.Tasks;

namespace Code.Tests.Generic
{
	internal static class TaskExtensions
	{
		internal static async Task TimeoutAfterSeconds(this Task task, byte seconds)
		{
			var delay = System.Diagnostics.Debugger.IsAttached
				? TimeSpan.FromMilliseconds(-1)
				: TimeSpan.FromSeconds(seconds);

			if (await Task.WhenAny(task, Task.Delay(delay)) == task)
				return;

			throw new Exception($"Timed out after {seconds} seconds.");
		}
	}
}
