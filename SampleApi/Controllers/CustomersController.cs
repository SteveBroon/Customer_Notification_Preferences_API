using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using SampleApi.Application.DTO;
using SampleApi.Application.Enums;
using SampleApi.Application.Interfaces;
namespace SampleApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomersController(
        ICustomerService customerService,
        IValidator<CustomerPreferencesRequest> requestValidator) : ControllerBase
    {
        [HttpGet]
        [Route("{customerId}/notification-preferences")]
        public async Task<ActionResult<CustomerPreferencesResponse>> GetNotificationPreferencesAsync(
            [FromRoute] Guid customerId,
            CancellationToken cancellationToken)
        {
            var preferences = await customerService.GetCustomerPreferencesAsync(customerId, cancellationToken);
            if (preferences == null)
            {
                return NotFound();
            }
            return Ok(preferences);
        }

        [HttpPut]
        [Route("{customerId}/notification-preferences")]
        public async Task<ActionResult<CustomerPreferencesResponse>> UpdateNotificationPreferencesAsync(
            [FromRoute] Guid customerId,
            [FromBody] CustomerPreferencesRequest request,
            CancellationToken cancellationToken)
        {
            var validation = await requestValidator.ValidateAsync(request, cancellationToken);
            if (!validation.IsValid)
            {
                foreach (var error in validation.Errors)
                {
                    ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
                }
                return ValidationProblem(ModelState);
            }

            var updateResult = await customerService.UpdateCustomerPreferencesAsync(new CustomerPreferences(customerId, request.EmailEnabled!.Value, request.SmsEnabled!.Value), cancellationToken);

            return HandleUpdateResult(updateResult, customerId, request);
        }

        private ActionResult<CustomerPreferencesResponse> HandleUpdateResult(
            UpdateOperationResult updateResult,
            Guid customerId,
            CustomerPreferencesRequest request)
        {
                return updateResult switch
                {
                    UpdateOperationResult.Success => Ok(new CustomerPreferencesResponse(
                        customerId,
                        request.EmailEnabled!.Value,
                        request.SmsEnabled!.Value
                    )),
                    UpdateOperationResult.NotFound => NotFound(),
                    UpdateOperationResult.Failure => Problem(
                        title: "Notification service unavailable",
                        detail: "Preferences were saved, but the notification system could not be informed.",
                        statusCode: StatusCodes.Status503ServiceUnavailable),
                    _ => throw new ArgumentOutOfRangeException(nameof(updateResult), updateResult, "Unexpected update result")
                };
        }

    }
}