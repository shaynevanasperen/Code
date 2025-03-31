﻿using System;
using System.Linq;
using System.Reflection;

namespace Code.Extensions.Object;

static partial class ObjectExtension
{
	internal static bool IsAtomic(this object? source) =>
		source == null ||
		source is string ||
		source is Enum ||
		source is DateTime ||
		source is decimal ||
		source.GetType().GetTypeInfo().IsPrimitive ||
		!source.GetType().GetRuntimeProperties().Any();
}