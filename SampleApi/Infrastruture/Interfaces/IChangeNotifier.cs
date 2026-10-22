namespace SampleApi.Infrastruture.Interfaces;

public interface IChangeNotifier
{
    Task<bool> NotifyCustomerPreferencesUpdateAsync(
        Guid customerId,
        bool emailEnabled,
        bool smsEnabled,
        CancellationToken cancellationToken);

}
