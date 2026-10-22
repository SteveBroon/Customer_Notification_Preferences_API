using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using SampleApi.Controllers;

namespace SampleApiTests
{
    public class CustomerControllerTests
    {
        private static readonly Guid CustomerId = Guid.Parse("6b56b62f-0bdf-4354-a822-9b1c257c5ec0");

        private CustomersController CreateController() => new();

        public class GetNotificationPreferencesAsyncTests : CustomerControllerTests
        {
            [Fact]
            public async Task ExistingCustomer_ReturnsNotificationPreferences()
            {
                var result = await CreateController().GetNotificationPreferencesAsync(CustomerId, CancellationToken.None);

                Assert.NotNull(result);
                Assert.IsType<OkObjectResult>(result);
            }   

            [Fact]
            public async Task NonExistingCustomer_ReturnsNotFound()
            {
                var result = await CreateController().GetNotificationPreferencesAsync(CustomerId, CancellationToken.None);

                Assert.NotNull(result);
                Assert.IsType<NotFoundResult>(result);
            }
        }
    }
}
