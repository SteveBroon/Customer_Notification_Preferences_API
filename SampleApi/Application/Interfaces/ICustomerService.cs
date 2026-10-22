using SampleApi.Application.DTO;
using SampleApi.Application.Enums;

namespace SampleApi.Application.Interfaces
{
    public interface ICustomerService
    {
    Task<CustomerPreferencesResponse?> GetCustomerPreferencesAsync(
        Guid customerId,
        CancellationToken cancellationToken);

    Task<UpdateOperationResult> UpdateCustomerPreferencesAsync(
        DTO.CustomerPreferences preferences,
        CancellationToken cancellationToken);

    }
}
