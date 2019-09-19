using System;
using System.Linq;

namespace Code.Extensions.ValueTuple.ServiceProvider
{
	partial class ValueTupleServiceProvider : IServiceProvider
	{
		readonly IServiceProvider _serviceProvider;

		public ValueTupleServiceProvider(IServiceProvider serviceProvider) => _serviceProvider = serviceProvider;

		public object GetService(System.Type serviceType) =>
			serviceType.IsValueTupleType()
				? Activator.CreateInstance(serviceType, serviceType
					.GetValueTupleItemTypes()
					.Select(GetService)
					.ToArray())
				: _serviceProvider.GetService(serviceType);
	}
}
