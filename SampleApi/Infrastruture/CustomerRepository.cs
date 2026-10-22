using SampleApi.Application.Interfaces;
using SampleApi.Application.DTO;
using Microsoft.EntityFrameworkCore;

namespace SampleApi.Infrastruture
{
    public class CustomerRepository(CustomerDbContext dbContext) : ICustomerRepository
    {
        public async Task<CustomerPreferencesResponse?> GetCustomerPreferencesAsync(Guid customerId, CancellationToken cancellationToken)
        {
            return await dbContext.Customers
                .AsNoTracking()
                .Where(c => c.Id == customerId)
                .Select(c => new CustomerPreferencesResponse(c.Id, c.EmailNotificationsEnabled, c.SmsNotificationsEnabled))
                .FirstOrDefaultAsync(cancellationToken);
        }
    }
}
