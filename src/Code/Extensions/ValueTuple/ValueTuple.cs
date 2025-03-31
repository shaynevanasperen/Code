using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Code.Extensions.ValueTuple;

static partial class ValueTupleExtension
{
	static readonly ConcurrentDictionary<System.Type, Lazy<FieldInfo[]>?> TuplesCache = new();

	static readonly HashSet<System.Type> ValueTupleTypes =
	[
		typeof(ValueTuple<>),
		typeof(ValueTuple<,>),
		typeof(ValueTuple<,,>),
		typeof(ValueTuple<,,,>),
		typeof(ValueTuple<,,,,>),
		typeof(ValueTuple<,,,,,>),
		typeof(ValueTuple<,,,,,,>),
		typeof(ValueTuple<,,,,,,,>)
	];

	internal static bool IsValueTuple(this object obj) => IsValueTupleType(obj.GetType());

	internal static bool IsValueTupleType(this System.Type type) =>
		TuplesCache.GetOrAdd(type, x => x.IsValueTupleTypeInternal()
			? new Lazy<FieldInfo[]>(() => GetItemFields(x))
			: null) != null;

	internal static object?[] GetValueTupleItemObjects(this object tuple) =>
		GetValueTupleItemFields(tuple.GetType()).Select(f => f.GetValue(tuple)).ToArray();

	internal static System.Type[] GetValueTupleItemTypes(this System.Type type) =>
		GetValueTupleItemFields(type).Select(f => f.FieldType).ToArray();

	static FieldInfo[] GetValueTupleItemFields(this System.Type type) =>
		TuplesCache.GetOrAdd(type, x => x.IsValueTupleTypeInternal()
			? new Lazy<FieldInfo[]>(() => GetItemFields(x))
			: null)?.Value ?? [];

	static bool IsValueTupleTypeInternal(this System.Type type) =>
		type.GetTypeInfo().IsGenericType && ValueTupleTypes.Contains(type.GetGenericTypeDefinition());

	static FieldInfo[] GetItemFields(System.Type key)
	{
		var items = new List<FieldInfo>();

		FieldInfo? field;
		var nth = 1;
		while ((field = key.GetRuntimeField($"Item{nth}")) != null)
		{
			nth++;
			items.Add(field);
		}

		return items.ToArray();
	}
}