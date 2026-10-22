using Microsoft.EntityFrameworkCore;

namespace SampleApi.Infrastruture
{
    public static class DependencyInjecttion
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services)
        {
            services.AddDbContext<CustomerDbContext>(options =>
                options.UseInMemoryDatabase("Customerdatabase"));

            return services;
        }
    }
}
