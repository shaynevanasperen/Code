using System;
using System.Linq;

namespace Code.Extensions.ValueTuple.ServiceProvider;

partial class ValueTupleServiceProvider(IServiceProvider serviceProvider) : IServiceProvider
{
	public object? GetService(System.Type serviceType) =>
		serviceType.IsValueTupleType()
			? Activator.CreateInstance(serviceType, serviceType
				.GetValueTupleItemTypes()
				.Select(GetService)
				.ToArray())
			: serviceProvider.GetService(serviceType);
}