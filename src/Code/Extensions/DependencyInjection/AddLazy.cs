using System;
using Microsoft.Extensions.DependencyInjection;

namespace Code.Extensions.DependencyInjection;

static partial class ServiceCollectionExtension
{
	internal static IServiceCollection AddLazy(this IServiceCollection services) => services.AddTransient(typeof(Lazy<>), typeof(Lazier<>));
}

class Lazier<T>(IServiceProvider provider) : Lazy<T>(provider.GetRequiredService<T>)
	where T : class;