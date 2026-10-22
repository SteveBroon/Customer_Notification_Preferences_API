namespace SampleApi.Application.DTO
{
    public record CustomerPreferencesResponse
    (
        Guid CustomerId,
        bool EmailEnabled,
        bool SmsEnabled
    );
}
