using Microsoft.AspNetCore.Mvc;
using SampleApi.Application.DTO;
using SampleApi.Application.Interfaces;
using SampleApi.Controllers;
using Moq;

namespace SampleApiTests
{
    public class CustomerControllerTests
    {
        private static readonly Guid CustomerId = Guid.Parse("6b56b62f-0bdf-4354-a822-9b1c257c5ec0");

        private readonly Mock<ICustomerRepository> customerRepository = new Mock<ICustomerRepository>();

        private CustomersController CreateController() => new(customerRepository.Object);

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

                customerRepository.Setup(r => r.GetCustomerPreferencesAsync(CustomerId, CancellationToken.None))
                    .ReturnsAsync(new CustomerPreferencesResponse(CustomerId, emailEnabled, smsEnabled));

                // Act
                var result = await CreateController().GetNotificationPreferencesAsync(CustomerId, CancellationToken.None);

                // Assert
                Assert.NotNull(result);
                var okResult = Assert.IsType<OkObjectResult>(result.Result);
                var actual = Assert.IsType<CustomerPreferencesResponse>(okResult.Value);
                Assert.True(expected == actual);
            }   

            [Fact]
            public async Task NonExistingCustomer_ReturnsNotFound()
            {
                // Arrange
                customerRepository.Setup(r => r.GetCustomerPreferencesAsync(CustomerId, CancellationToken.None))
                    .ReturnsAsync((CustomerPreferencesResponse?)null);

                // Act
                var result = await CreateController().GetNotificationPreferencesAsync(CustomerId, CancellationToken.None);

                // Assert
                Assert.NotNull(result);
                Assert.IsType<NotFoundResult>(result.Result);
            }
        }
    }
}
