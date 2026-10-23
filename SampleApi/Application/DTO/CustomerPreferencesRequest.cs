namespace SampleApi.Application.DTO
{
    public class CustomerPreferencesRequest
    {
        public required bool EmailEnabled { get; init; }
        public required bool SmsEnabled { get; init; }
    }
}
