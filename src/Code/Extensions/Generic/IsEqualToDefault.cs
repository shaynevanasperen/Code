namespace Code.Extensions.Generic
{
	static partial class GenericExtension
	{
		internal static bool IsEqualToDefault<T>(this T obj)
		{
			return Equals(obj, default(T));
		}
	}
}
