using System;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace Code.Extensions.Exception;

/// <summary>
/// Based on https://github.com/justeat/NLog.StructuredLogging.Json/blob/6bbe30b1c256a6b32f2b3ad898f8438ae1d11ea2/src/NLog.StructuredLogging.Json/Helpers/ConvertException.cs
/// </summary>
static partial class ExceptionExtension
{
	/// <summary>
	/// Returns a deterministic string "hash" of the given <see cref="Exception"/> so that we can easily identity this in logs.
	/// </summary>
	/// <param name="exception">The exception instance to create a fingerprint from</param>
	/// <returns>A deterministic string "hash" of the given <see cref="Exception"/>.</returns>
	internal static string GetFingerprint(this System.Exception? exception)
	{
		if (exception == null)
			return string.Empty;

		var canonical = CanonicalizeException(exception);
		return HashString(canonical);
	}

	static string CanonicalizeException(System.Exception exception)
	{
		try
		{
			if (exception.StackTrace != null)
				return StackTraceWithoutVariantFilePaths(exception);

			return exception.Message;
		}
#pragma warning disable CA1031 // Do not catch general exception types
		catch
#pragma warning restore CA1031 // Do not catch general exception types
		{
			return string.Empty;
		}
	}

	static string HashString(string value)
	{
		var buffer = Encoding.UTF8.GetBytes(value);
		byte[] hash;

#pragma warning disable CA5350 // Do Not Use Weak Cryptographic Algorithms
		using (var algorithm = SHA1.Create())
#pragma warning restore CA5350 // Do Not Use Weak Cryptographic Algorithms
		{
			hash = algorithm.ComputeHash(buffer);
		}

		var builder = new StringBuilder(hash.Length * 2);

		foreach (var b in hash)
		{
			builder.Append(b.ToString("x2", CultureInfo.InvariantCulture));
		}

		return builder.ToString();
	}

	static string StackTraceWithoutVariantFilePaths(System.Exception exception)
	{
		var firstFrame = true;
		var stackTrace = new StackTrace(exception, true);
		var frames = stackTrace.GetFrames();
		var builder = new StringBuilder(255);

		// ReSharper disable once PossibleNullReferenceException
		foreach (var stackFrame in frames)
		{
			var method = stackFrame.GetMethod();

			if (method != null)
			{
				if (!firstFrame)
				{
					builder.Append(Environment.NewLine);
				}

				firstFrame = false;

				builder.Append("   at ");

				AddMethodName(method, builder);
				AddGenericParameters(method, builder);
				AddParameters(method, builder);
				AddLineNumber(stackFrame, builder);
			}
		}

		return builder.ToString();
	}

	static void AddMethodName(MethodBase method, StringBuilder builder)
	{
		var declaringType = method.DeclaringType;

		if (declaringType != null)
		{
			// ReSharper disable once PossibleNullReferenceException
			builder.Append(declaringType.FullName?.Replace('+', '.'));
			builder.Append('.');
		}

		builder.Append(method.Name);
	}

	static void AddGenericParameters(MethodBase method, StringBuilder builder)
	{
		if (method is MethodInfo methodInfo && methodInfo.IsGenericMethod)
		{
			var genericArguments = methodInfo.GetGenericArguments();
			builder.Append('[');

			var firstGenericArgument = true;

			foreach (var genericArgument in genericArguments)
			{
				if (!firstGenericArgument)
				{
					builder.Append(',');
				}

				firstGenericArgument = false;

				builder.Append(genericArgument.Name);
			}

			builder.Append(']');
		}
	}

	static void AddParameters(MethodBase method, StringBuilder builder)
	{
		builder.Append('(');

		var parameterInfos = method.GetParameters();
		var firstParam = true;

		foreach (var parameterInfo in parameterInfos)
		{
			if (!firstParam)
			{
				builder.Append(", ");
			}

			firstParam = false;

			var typeName = parameterInfo.ParameterType.Name;
			builder.Append(typeName + " " + parameterInfo.Name);
		}

		builder.Append(')');
	}

	static void AddLineNumber(StackFrame stackFrame, StringBuilder builder)
	{
		var lineNumber = stackFrame.GetFileLineNumber();

		if (lineNumber != 0)
			builder.Append(CultureInfo.InvariantCulture, $" line: {stackFrame.GetFileLineNumber()}");
	}
}