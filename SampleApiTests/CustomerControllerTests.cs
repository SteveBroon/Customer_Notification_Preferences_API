using Microsoft.AspNetCore.Mvc;
using SampleApi.Application.DTO;
using SampleApi.Controllers;

namespace SampleApiTests
{
    public class CustomerControllerTests
    {
        private static readonly Guid CustomerId = Guid.Parse("6b56b62f-0bdf-4354-a822-9b1c257c5ec0");

        private CustomersController CreateController() => new();

        public class GetNotificationPreferencesAsyncTests : CustomerControllerTests
        {
            [Theory]
            [InlineData(true, true)]
            [InlineData(true, false)]
            [InlineData(false, true)]
            [InlineData(false, false)]
            public async Task ExistingCustomer_ReturnsNotificationPreferences(bool emailEnabled, bool smsEnabled)
            {
                // Arrange
                var expected = new CustomerPreferencesResponse(CustomerId, emailEnabled, smsEnabled);

                // Act
                var result = await CreateController().GetNotificationPreferencesAsync(CustomerId, CancellationToken.None);

                // Assert
                Assert.NotNull(result);
                var okResult = Assert.IsType<OkObjectResult>(result);
                Assert.Same(expected, okResult.Value);
            }   

            [Fact]
            public async Task NonExistingCustomer_ReturnsNotFound()
            {
                // Arrange

                // Act
                var result = await CreateController().GetNotificationPreferencesAsync(CustomerId, CancellationToken.None);

                // Assert
                Assert.NotNull(result);
                Assert.IsType<NotFoundResult>(result);
            }
        }
    }
}
