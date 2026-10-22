namespace SampleApi.Application.DTO
{
    public record CustomerPreferences (
        Guid CustomerId,
        bool EmailEnabled,
        bool SmsEnabled
    );
}
