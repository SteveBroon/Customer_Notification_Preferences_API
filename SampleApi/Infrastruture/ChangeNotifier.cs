using SampleApi.Infrastruture.Interfaces;

namespace SampleApi.Infrastruture
{
    public class ChangeNotifier(ILogger<ChangeNotifier> logger) : IChangeNotifier
    {
        public async Task<bool> NotifyCustomerPreferencesUpdateAsync(
            Guid customerId,
            bool emailEnabled,
            bool smsEnabled,
            CancellationToken cancellationToken)
        {
            // Implement the notification logic here
            logger.LogInformation("Notifying change of preferences for customer {CustomerId} : EmailEnabled={EmailEnabled}, SmsEnabled={SmsEnabled}", customerId, emailEnabled, smsEnabled);
            return await Task.FromResult(true);
        }
    }
}
