using FluentValidation;
using SampleApi.Application.DTO;
using SampleApi.Application.Interfaces;
using SampleApi.Application.Services;
using SampleApi.Application.Validation;

namespace SampleApi.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddSingleton<IValidator<CustomerPreferencesRequest>, CustomerPreferencesRequestValidator>();
        services.AddScoped<ICustomerService, CustomerService>();
        return services;
    }
}
