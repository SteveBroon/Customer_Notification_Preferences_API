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
            throw new NotImplementedException();
        }
    }
}
