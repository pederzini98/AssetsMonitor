// See https://aka.ms/new-console-template for more information
using Microsoft.Extensions.DependencyInjection;


namespace Presentation
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddPresentation(this IServiceCollection services)
        {
            return services;
        }
    }
}
