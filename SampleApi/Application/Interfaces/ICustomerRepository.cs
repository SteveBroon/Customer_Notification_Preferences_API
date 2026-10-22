using SampleApi.Application.DTO;
using SampleApi.Application.Enums;

namespace SampleApi.Application.Interfaces;

public interface ICustomerRepository
{
    Task<CustomerPreferencesResponse?> GetCustomerPreferencesAsync(Guid customerId, CancellationToken cancellationToken);
    Task<UpdatePreferencesResult> UpdateCustomerPreferencesAsync(CustomerPreferences preferences, CancellationToken cancellationToken);
}
