using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            var assemblyObj = typeof(DependencyInjection).Assembly;
            services.AddMediatR(sysConfig =>
                sysConfig.RegisterServicesFromAssembly(assemblyObj));


            services.AddValidatorsFromAssembly(assemblyObj);
            services.AddHttpClient<MonitoringData>();
            services.AddSingleton<IMonitoringData, MonitoringData>();

            return services;

        }
    }
}