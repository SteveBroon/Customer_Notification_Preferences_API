using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using SampleApi.Application.DTO;
using SampleApi.Application.Interfaces;
namespace SampleApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomersController(
        ICustomerRepository customerRepository,
        IValidator<CustomerPreferencesRequest> requestValidator) : ControllerBase
    {
        [HttpGet]
        [Route("{customerId}/notification-preferences")]
        public async Task<ActionResult<CustomerPreferencesResponse>> GetNotificationPreferencesAsync(
            [FromRoute] Guid customerId,
            CancellationToken cancellationToken)
        {
            var preferences = await customerRepository.GetCustomerPreferencesAsync(customerId, cancellationToken);
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

            var updated = await customerRepository.UpdateCustomerPreferencesAsync(customerId, request, cancellationToken);
            if (!updated)
            {
                return NotFound();
            }
            return Ok(new CustomerPreferencesResponse(customerId, request.EmailEnabled!.Value, request.SmsEnabled!.Value));
        }
    }
}
