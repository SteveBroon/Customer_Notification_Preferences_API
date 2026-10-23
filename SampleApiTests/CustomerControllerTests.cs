using Microsoft.AspNetCore.Mvc;
using SampleApi.Application.DTO;
using SampleApi.Application.Interfaces;
using SampleApi.Controllers;
using Moq;
using SampleApi.Application.Enums;

namespace SampleApiTests
{
    public class CustomerControllerTests
    {
        private static readonly Guid CustomerId = Guid.Parse("6b56b62f-0bdf-4354-a822-9b1c257c5ec0");

        private readonly Mock<ICustomerService> customerService = new Mock<ICustomerService>();

        private CustomersController CreateController() => new(customerService.Object);

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

                customerService.Setup(r => r.GetCustomerPreferencesAsync(CustomerId, CancellationToken.None))
                    .ReturnsAsync(new CustomerPreferencesResponse(CustomerId, emailEnabled, smsEnabled));

                // Act
                var result = await CreateController().GetNotificationPreferencesAsync(CustomerId, CancellationToken.None);

                // Assert
                Assert.NotNull(result);
                var okResult = Assert.IsType<OkObjectResult>(result.Result);
                var actual = Assert.IsType<CustomerPreferencesResponse>(okResult.Value);
                Assert.True(expected == actual);
                customerService.Verify(
                    r => r.GetCustomerPreferencesAsync(CustomerId, It.IsAny<CancellationToken>()),
                    Times.Once);
            }   

            [Fact]
            public async Task NonExistingCustomer_ReturnsNotFound()
            {
                // Arrange
                customerService.Setup(r => r.GetCustomerPreferencesAsync(CustomerId, CancellationToken.None))
                    .ReturnsAsync((CustomerPreferencesResponse?)null);

                // Act
                var result = await CreateController().GetNotificationPreferencesAsync(CustomerId, CancellationToken.None);

                // Assert
                Assert.NotNull(result);
                Assert.IsType<NotFoundResult>(result.Result);
                customerService.Verify(
                    r => r.GetCustomerPreferencesAsync(CustomerId, It.IsAny<CancellationToken>()),
                    Times.Once);
            }
        }

        public class UpdateNotificationPreferencesAsyncTests : CustomerControllerTests
        {
            [Theory]
            [InlineData(true, true)]
            [InlineData(true, false)]
            [InlineData(false, true)]
            [InlineData(false, false)]
            public async Task ExistingCustomer_WithChangedPreferences_ReturnsUpdatedPreferences(bool emailEnabled, bool smsEnabled)
            {
                // Arrange
                var updateRequest = new CustomerPreferencesRequest
                {
                    EmailEnabled = emailEnabled,
                    SmsEnabled = smsEnabled
                };
                var expected = new CustomerPreferencesResponse
                (
                    CustomerId,
                    emailEnabled,
                    smsEnabled
                );

                customerService
                    .Setup(r => r.UpdateCustomerPreferencesAsync(new CustomerPreferences(CustomerId, updateRequest.EmailEnabled, updateRequest.SmsEnabled), CancellationToken.None))
                    .ReturnsAsync(UpdateOperationResult.Success);

                // Act
                var result = await CreateController().UpdateNotificationPreferencesAsync(CustomerId, updateRequest, CancellationToken.None);

                // Assert
                Assert.NotNull(result);
                var okResult = Assert.IsType<OkObjectResult>(result.Result);
                var actual = Assert.IsType<CustomerPreferencesResponse>(okResult.Value);
                Assert.True(actual == expected);
                customerService.Verify(
                    r => r.UpdateCustomerPreferencesAsync(new CustomerPreferences(CustomerId, updateRequest.EmailEnabled, updateRequest.SmsEnabled), CancellationToken.None),
                    Times.Once);
            }

            [Fact]
            public async Task NonExistingCustomer_ReturnsNotFound()
            {
                // Arrange
                var updateRequest = new CustomerPreferencesRequest
                {
                    EmailEnabled = true,
                    SmsEnabled = true
                };

                customerService
                    .Setup(r => r.UpdateCustomerPreferencesAsync(new CustomerPreferences(CustomerId, updateRequest.EmailEnabled, updateRequest.SmsEnabled), CancellationToken.None))
                    .ReturnsAsync(UpdateOperationResult.NotFound);

                // Act
                var result = await CreateController().UpdateNotificationPreferencesAsync(CustomerId, updateRequest, CancellationToken.None);

                // Assert
                Assert.NotNull(result);
                Assert.IsType<NotFoundResult>(result.Result);
                customerService.Verify(
                    r => r.UpdateCustomerPreferencesAsync(new CustomerPreferences(CustomerId, updateRequest.EmailEnabled, updateRequest.SmsEnabled), CancellationToken.None),
                    Times.Once);
            }

            [Fact]
            public async Task NotificationServiceFails_ReturnsServiceUnavailable()
            {
                // Arrange
                var updateRequest = new CustomerPreferencesRequest
                {
                    EmailEnabled = true,
                    SmsEnabled = true
                };

                customerService
                    .Setup(r => r.UpdateCustomerPreferencesAsync(new CustomerPreferences(CustomerId, updateRequest.EmailEnabled, updateRequest.SmsEnabled), CancellationToken.None))
                    .ReturnsAsync(UpdateOperationResult.Failure);

                // Act
                var result = await CreateController().UpdateNotificationPreferencesAsync(CustomerId, updateRequest, CancellationToken.None);

                // Assert
                Assert.NotNull(result);
                Assert.IsType<ObjectResult>(result.Result);
                var objectResult = result.Result as ObjectResult;
                Assert.Equal(503, objectResult!.StatusCode);
                Assert.Equal("Notification service unavailable", objectResult!.Value?.GetType().GetProperty("Title")?.GetValue(objectResult.Value));
                Assert.Equal("Preferences were saved, but the notification system could not be informed.", objectResult!.Value?.GetType().GetProperty("Detail")?.GetValue(objectResult.Value));

                customerService.Verify(
                    r => r.UpdateCustomerPreferencesAsync(new CustomerPreferences(CustomerId, updateRequest.EmailEnabled, updateRequest.SmsEnabled), CancellationToken.None),
                    Times.Once);
            }
        }
    }
}
