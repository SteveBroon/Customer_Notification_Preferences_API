using SampleApi.Application.DTO;
using SampleApi.Application.Validation;

namespace SampleApiTests;

public class CustomerPreferencesRequestValidationTests
{
private readonly CustomerPreferencesRequestValidator _validator = new();

[Theory]
[InlineData(true, true)]
[InlineData(true, false)]
[InlineData(false, true)]
[InlineData(false, false)]
public void BothValuesProvided_IsValid(
    bool email,
    bool sms)
{
    var request = new CustomerPreferencesRequest(email, sms);
    
    var result = _validator.Validate(request);

    Assert.True(result.IsValid);
}

[Fact]
public void MissingEmail_FailsWithEmailMessage()
{
    var request = new CustomerPreferencesRequest(null, true);

    var result = _validator.Validate(request);

    var error = Assert.Single(result.Errors);
    Assert.Equal(nameof(CustomerPreferencesRequest.EmailEnabled), error.PropertyName);
    Assert.Equal("Email preference is required.", error.ErrorMessage);
}

[Fact]
public void MissingSms_FailsWithSmsMessage()
{
    var request = new CustomerPreferencesRequest(true, null);

    var result = _validator.Validate(request);

    var error = Assert.Single(result.Errors);
    Assert.Equal(nameof(CustomerPreferencesRequest.SmsEnabled), error.PropertyName);
    Assert.Equal("SMS preference is required.", error.ErrorMessage);
}

[Fact]
public void BothMissing_ReportsBothErrors()
{
    var request = new CustomerPreferencesRequest(null, null);
    
    var result = _validator.Validate(request);

    Assert.Equal(2, result.Errors.Count);
    Assert.Collection(
        result.Errors,
        error =>
        {
            Assert.Equal(nameof(CustomerPreferencesRequest.EmailEnabled), error.PropertyName);
            Assert.Equal("Email preference is required.", error.ErrorMessage);
        },
        error =>
        {
            Assert.Equal(nameof(CustomerPreferencesRequest.SmsEnabled), error.PropertyName);
            Assert.Equal("SMS preference is required.", error.ErrorMessage);
        });
}
}
