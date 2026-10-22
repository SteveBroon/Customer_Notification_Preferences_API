using FluentValidation;
using SampleApi.Application.DTO;

namespace SampleApi.Application.Validation;

public class CustomerPreferencesRequestValidator : AbstractValidator<CustomerPreferencesRequest>
{
    public CustomerPreferencesRequestValidator()
    {
        RuleFor(x => x.EmailEnabled).NotNull().WithMessage("Email preference is required.");
        RuleFor(x => x.SmsEnabled).NotNull().WithMessage("SMS preference is required.");
    }
}
