using SampleApi.Infrastruture.Interfaces;

namespace SampleApi.Infrastruture;

public class ChangeNotifier : IChangeNotifier
{
    public async Task<bool> NotifyCustomerPreferencesUpdateAsync(
        Guid customerId,
        bool emailEnabled,
        bool smsEnabled,
        CancellationToken cancellationToken)
    {
        // Implement the notification logic here
        return await Task.FromResult(true);
    }
}
