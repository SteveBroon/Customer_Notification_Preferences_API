using Microsoft.AspNetCore.Mvc;
using SampleApi.Application.DTO;
using SampleApi.Application.Interfaces;
using SampleApi.Controllers;
using Moq;
using FluentValidation;
using FluentValidation.Results;
using SampleApi.Application.Enums;

namespace SampleApiTests
{
    public class CustomerControllerTests
    {
        private static readonly Guid CustomerId = Guid.Parse("6b56b62f-0bdf-4354-a822-9b1c257c5ec0");

        private readonly Mock<ICustomerService> customerService = new Mock<ICustomerService>();
        private readonly Mock<IValidator<CustomerPreferencesRequest>> requestValidator = new(MockBehavior.Strict);

        private CustomersController CreateController() => new(customerService.Object, requestValidator.Object);

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
                requestValidator.VerifyNoOtherCalls();
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
                requestValidator.VerifyNoOtherCalls();
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
                (
                    emailEnabled,
                    smsEnabled
                );
                var expected = new CustomerPreferencesResponse
                (
                    CustomerId,
                    emailEnabled,
                    smsEnabled
                );

                requestValidator
                    .Setup(v => v.ValidateAsync(updateRequest, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new ValidationResult());

                customerService
                    .Setup(r => r.UpdateCustomerPreferencesAsync(new CustomerPreferences(CustomerId, updateRequest.EmailEnabled!.Value, updateRequest.SmsEnabled!.Value), CancellationToken.None))
                    .ReturnsAsync(UpdateOperationResult.Success);

                // Act
                var result = await CreateController().UpdateNotificationPreferencesAsync(CustomerId, updateRequest, CancellationToken.None);

                // Assert
                Assert.NotNull(result);
                var okResult = Assert.IsType<OkObjectResult>(result.Result);
                var actual = Assert.IsType<CustomerPreferencesResponse>(okResult.Value);
                Assert.True(actual == expected);
                requestValidator.Verify(
                    v => v.ValidateAsync(updateRequest, It.IsAny<CancellationToken>()),
                    Times.Once);
                customerService.Verify(
                    r => r.UpdateCustomerPreferencesAsync(new CustomerPreferences(CustomerId, updateRequest.EmailEnabled!.Value, updateRequest.SmsEnabled!.Value), CancellationToken.None),
                    Times.Once);
            }

            [Fact]
            public async Task NonExistingCustomer_ReturnsNotFound()
            {
                // Arrange
                var updateRequest = new CustomerPreferencesRequest
                (
                    true,
                    true
                );

                requestValidator
                    .Setup(v => v.ValidateAsync(updateRequest, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new ValidationResult());

                customerService
                    .Setup(r => r.UpdateCustomerPreferencesAsync(new CustomerPreferences(CustomerId, updateRequest.EmailEnabled!.Value, updateRequest.SmsEnabled!.Value), CancellationToken.None))
                    .ReturnsAsync(UpdateOperationResult.NotFound);

                // Act
                var result = await CreateController().UpdateNotificationPreferencesAsync(CustomerId, updateRequest, CancellationToken.None);

                // Assert
                Assert.NotNull(result);
                Assert.IsType<NotFoundResult>(result.Result);
                requestValidator.Verify(
                    v => v.ValidateAsync(updateRequest, It.IsAny<CancellationToken>()),
                    Times.Once);
                customerService.Verify(
                    r => r.UpdateCustomerPreferencesAsync(new CustomerPreferences(CustomerId, updateRequest.EmailEnabled!.Value, updateRequest.SmsEnabled!.Value), CancellationToken.None),
                    Times.Once);
            }

            [Fact]
            public async Task NotificationServiceFails_ReturnsServiceUnavailable()
            {
                // Arrange
                var updateRequest = new CustomerPreferencesRequest(true, true);

                requestValidator
                    .Setup(v => v.ValidateAsync(updateRequest, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new ValidationResult());

                customerService
                    .Setup(r => r.UpdateCustomerPreferencesAsync(new CustomerPreferences(CustomerId, updateRequest.EmailEnabled!.Value, updateRequest.SmsEnabled!.Value), CancellationToken.None))
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

                requestValidator.Verify(
                    v => v.ValidateAsync(updateRequest, It.IsAny<CancellationToken>()),
                    Times.Once);
                customerService.Verify(
                    r => r.UpdateCustomerPreferencesAsync(new CustomerPreferences(CustomerId, updateRequest.EmailEnabled!.Value, updateRequest.SmsEnabled!.Value), CancellationToken.None),
                    Times.Once);
            }

            [Fact]
            public async Task InvalidPreferences_ReturnsBadRequest()
            {
                // Arrange
                var updateRequest = new CustomerPreferencesRequest(true, true);

                requestValidator
                    .Setup(v => v.ValidateAsync(It.IsAny<CustomerPreferencesRequest>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new ValidationResult(new[] { new ValidationFailure("EmailEnabled", "Invalid") }));

                customerService
                    .Setup(r => r.UpdateCustomerPreferencesAsync(new CustomerPreferences(CustomerId, updateRequest.EmailEnabled!.Value, updateRequest.SmsEnabled!.Value), CancellationToken.None))
                    .ReturnsAsync(UpdateOperationResult.Success);

                // Act
                var result = await CreateController().UpdateNotificationPreferencesAsync(CustomerId, updateRequest, CancellationToken.None);

                // Assert
                Assert.NotNull(result);
                var problemDetails = Assert.IsType<ValidationProblemDetails>(Assert.IsType<ObjectResult>(result.Result).Value);
                Assert.Equal(new[] { "Invalid" }, problemDetails.Errors["EmailEnabled"]);

                requestValidator.Verify(
                    v => v.ValidateAsync(It.IsAny<CustomerPreferencesRequest>(), It.IsAny<CancellationToken>()),
                    Times.Once);
                customerService.VerifyNoOtherCalls();
            }

        }
    }
}
