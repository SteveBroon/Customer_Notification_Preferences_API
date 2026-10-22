using SampleApi.Application.Interfaces;
using SampleApi.Application.DTO;
using Microsoft.EntityFrameworkCore;
using SampleApi.Application.Enums;

namespace SampleApi.Infrastruture;

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

    public async Task<UpdatePreferencesResult> UpdateCustomerPreferencesAsync(CustomerPreferences preferences, CancellationToken cancellationToken)
    {
        var customer = await dbContext.Customers.FirstOrDefaultAsync(c => c.Id == preferences.CustomerId, cancellationToken);
        if (customer != null)
        {
            if (customer.EmailNotificationsEnabled == preferences.EmailEnabled && customer.SmsNotificationsEnabled == preferences.SmsEnabled)
            {
                return UpdatePreferencesResult.UnChanged;
            }

            customer.EmailNotificationsEnabled = preferences.EmailEnabled;
            customer.SmsNotificationsEnabled = preferences.SmsEnabled;
            await dbContext.SaveChangesAsync(cancellationToken);
            return UpdatePreferencesResult.Updated;
        }
        return UpdatePreferencesResult.CustomerNotFound;
    }
}
