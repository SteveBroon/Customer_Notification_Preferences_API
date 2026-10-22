using FluentValidation;
using SampleApi.Application.DTO;
using SampleApi.Application.Validation;

namespace SampleApi.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddSingleton<IValidator<CustomerPreferencesRequest>, CustomerPreferencesRequestValidator>();
            return services;
        }
    }
}
