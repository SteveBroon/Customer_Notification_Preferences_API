namespace SampleApi.Application.DTO;

public record CustomerPreferencesRequest(
    bool? EmailEnabled,
    bool? SmsEnabled
);
