namespace Code.Extensions.Generic;

static partial class GenericExtension
{
	internal static bool IsEqualToDefault<T>(this T obj) => Equals(obj, default(T));
}