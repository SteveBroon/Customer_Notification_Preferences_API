using Microsoft.EntityFrameworkCore;
using SampleApi.Application.Interfaces;
using SampleApi.Infrastruture.Interfaces;

namespace SampleApi.Infrastruture
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services)
        {
            services.AddDbContext<CustomerDbContext>(options =>
                options.UseInMemoryDatabase("Customerdatabase"));

            services.AddScoped<ICustomerRepository, CustomerRepository>();
            services.AddSingleton<IChangeNotifier, ChangeNotifier>();

            return services;
        }

        public static IHost InitialiseDatabase(this IHost host)
        {
            using var scope = host.Services.CreateScope();
            scope.ServiceProvider.GetRequiredService<CustomerDbContext>().Database.EnsureCreated();

            return host;
        }

    }
}
