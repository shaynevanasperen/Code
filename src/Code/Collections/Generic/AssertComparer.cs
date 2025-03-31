using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Code.Extensions.Generic;
using Code.Extensions.Object;
using Code.Extensions.Type;

namespace Code.Collections.Generic;

class AssertComparer<T> : IComparer<T>, IEqualityComparer<T>
{
	public int Compare(T? x, T? y)
	{
		foreach (var comparerStrategy in ComparerFactory.GetComparers<T>())
		{
			var comparisionResult = comparerStrategy.Compare(x, y);
			if (comparisionResult.FoundResult)
				return comparisionResult.Result;
		}
		return ComparerFactory.GetDefaultComparer<T>().Compare(x, y);
	}

	public bool Equals(T? x, T? y) => Compare(x, y) == 0;

	public int GetHashCode([DisallowNull] T obj) => obj.GetHashCode();
}

static class ComparerFactory
{
	internal static IEnumerable<IComparerStrategy<T>> GetComparers<T>() =>
	[
		new EnumerableComparer<T>(),
		new GenericTypeComparer<T>(),
		new ComparableComparer<T>(),
		new EquatableComparer<T>(),
		new TypeComparer<T>(),
		new ReflectionComparer<T>()
	];

	internal static IComparer<T> GetDefaultComparer<T>() => new DefaultComparer<T>();
}

interface IComparerStrategy<in T>
{
	ComparisionResult Compare(T? x, T? y);
}

class ComparisionResult
{
	public bool FoundResult { get; protected init; }

	public int Result { get; }

	public ComparisionResult(int result)
	{
		FoundResult = true;
		Result = result;
	}

	protected ComparisionResult()
	{
	}
}

class NoResult : ComparisionResult
{
	public NoResult() => FoundResult = false;
}

class DefaultComparer<T> : IComparer<T>
{
	public int Compare(T? x, T? y) => !Equals(x, y) ? -1 : 0;
}

class EnumerableComparer<T> : IComparerStrategy<T>
{
	public ComparisionResult Compare(T? x, T? y)
	{
		if (x is not IEnumerable enumerable1 || y is not IEnumerable enumerable2)
			return new NoResult();
		var enumerator1 = enumerable1.GetEnumerator();
		var enumerator2 = enumerable2.GetEnumerator();
		do
		{
			var item1 = enumerator1.MoveNext();
			var item2 = enumerator2.MoveNext();
			if (!item1 || !item2)
				return new(item1 == item2 ? 0 : -1);
		} while (Equals(enumerator1.Current, enumerator2.Current));
		return new(-1);
	}
}

class GenericTypeComparer<T> : IComparerStrategy<T>
{
	public ComparisionResult Compare(T? x, T? y)
	{
		var type = typeof(T);
		if (!type.GetTypeInfo().IsValueType || type.GetTypeInfo().IsGenericType && type.IsNullable())
		{
			if (x.IsEqualToDefault())
				return new(y.IsEqualToDefault() ? 0 : -1);
			if (y.IsEqualToDefault())
				return new(-1);
		}
		return new NoResult();
	}
}

class ComparableComparer<T> : IComparerStrategy<T>
{
	public ComparisionResult Compare(T? x, T? y)
	{
		if (x is IComparable<T> comparable1)
			return new(comparable1.CompareTo(y));
		if (x is IComparable comparable2)
			return new(comparable2.CompareTo(y));
		return new NoResult();
	}
}

class EquatableComparer<T> : IComparerStrategy<T>
{
	public ComparisionResult Compare(T? x, T? y)
	{
		if (x is not IEquatable<T> equatable)
			return new NoResult();
		return new(equatable.Equals(y) ? 0 : -1);
	}
}

class TypeComparer<T> : IComparerStrategy<T>
{
	public ComparisionResult Compare(T? x, T? y)
	{
		if (x?.GetType() != y?.GetType())
			return new(-1);
		return new NoResult();
	}
}

class ReflectionComparer<T> : IComparerStrategy<T>
{
	public ComparisionResult Compare(T? x, T? y)
	{
		try
		{
			var xString = string.Join("_", x.Flatten());
			var yString = string.Join("_", y.Flatten());
			return new(string.CompareOrdinal(xString, yString));
		}
#pragma warning disable CA1031 // Do not catch general exception types
		catch
#pragma warning restore CA1031 // Do not catch general exception types
		{
			return new NoResult();
		}
	}
}