using System;
using System.Threading;
using System.Threading.Tasks;

namespace Code.Threading;

static class SafeAsync
{
	/// <summary>
	/// Returns the result from a task, preventing deadlocks due to the presence of a synchronization context.
	/// Use as a temporary solution until you can flow async all the way through.
	/// </summary>
	/// <param name="asyncOperation">The asynchronous operation.</param>
	/// <typeparam name="T">The type of the result.</typeparam>
	/// <returns>The result of the task.</returns>
	internal static T GetResultSafely<T>(this Func<Task<T>> asyncOperation)
	{
		var existingContext = SynchronizationContext.Current;
		try
		{
			SynchronizationContext.SetSynchronizationContext(new());
			return asyncOperation().Result;
		}
		finally
		{
			SynchronizationContext.SetSynchronizationContext(existingContext);
		}
	}

	/// <summary>
	/// Waits for a task to complete, preventing deadlocks due to the presence of a synchronization context.
	/// Use as a temporary solution until you can flow async all the way through.
	/// </summary>
	/// <param name="asyncOperation">The asynchronous operation.</param>
	internal static void WaitSafely(this Func<Task> asyncOperation)
	{
		var existingContext = SynchronizationContext.Current;
		try
		{
			SynchronizationContext.SetSynchronizationContext(new());
			asyncOperation().Wait();
		}
		finally
		{
			SynchronizationContext.SetSynchronizationContext(existingContext);
		}
	}
}