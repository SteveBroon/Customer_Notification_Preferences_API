using SampleApi.Application.DTO;
using SampleApi.Application.Enums;
using SampleApi.Application.Interfaces;
using SampleApi.Infrastruture.Interfaces;

namespace SampleApi.Application.Services
{
    public class CustomerService(
        ICustomerRepository customerRepository,
        IChangeNotifier changeNotifier,
        ILogger<CustomerService> logger
    ) : ICustomerService
    {
        public async Task<CustomerPreferencesResponse?> GetCustomerPreferencesAsync(
            Guid customerId,
            CancellationToken cancellationToken)
        {
            var response = await customerRepository.GetCustomerPreferencesAsync(customerId, cancellationToken);
            if (response == null)
            {
                return null;
            }
            return new CustomerPreferencesResponse(
                response.CustomerId,
                response.EmailEnabled,
                response.SmsEnabled
            );
        }

        public async Task<UpdateOperationResult> UpdateCustomerPreferencesAsync(
            CustomerPreferences preferences,
            CancellationToken cancellationToken)
        {
            var result = await customerRepository.UpdateCustomerPreferencesAsync(
                preferences,
                cancellationToken);
            if (result == UpdatePreferencesResult.Updated)
            {
                try
                {
                    var notificationResult = await changeNotifier.NotifyCustomerPreferencesUpdateAsync(
                        preferences.CustomerId,
                        preferences.EmailEnabled,
                        preferences.SmsEnabled,
                        cancellationToken);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to notify customer preferences update for customer {CustomerId}", preferences.CustomerId);
                    return UpdateOperationResult.Failure;
                }           
            }

            return MapUpdatePreferencesResult(result);
        }

        private static UpdateOperationResult MapUpdatePreferencesResult(UpdatePreferencesResult updateResult) => updateResult switch
        {
            UpdatePreferencesResult.Updated => UpdateOperationResult.Success,
            UpdatePreferencesResult.Unchanged => UpdateOperationResult.Success,
            UpdatePreferencesResult.CustomerNotFound => UpdateOperationResult.NotFound,
            _ => throw new ArgumentOutOfRangeException(nameof(updateResult), updateResult, "Unexpected update result")
        };

    }
}
