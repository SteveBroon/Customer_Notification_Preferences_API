using SampleApi.Application.DTO;

namespace SampleApi.Application.Interfaces
{
    public interface ICustomerRepository
    {
        Task<CustomerPreferencesResponse?> GetCustomerPreferencesAsync(Guid customerId, CancellationToken cancellationToken);
        Task<bool> UpdateCustomerPreferencesAsync(Guid customerId, CustomerPreferencesRequest request, CancellationToken cancellationToken);
    }
}
