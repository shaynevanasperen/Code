using System;
using Microsoft.Extensions.DependencyInjection;

namespace Code.Extensions.DependencyInjection
{
	static partial class ServiceCollectionExtension
	{
		internal static IServiceCollection AddLazy(this IServiceCollection services) => services.AddTransient(typeof(Lazy<>), typeof(Lazier<>));
	}

	class Lazier<T> : Lazy<T> where T : class
	{
		public Lazier(IServiceProvider provider) : base(provider.GetRequiredService<T>) { }
	}
}
