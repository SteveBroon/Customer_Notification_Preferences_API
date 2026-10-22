using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using SampleApi.Application.DTO;
using SampleApi.Application.Enums;
using SampleApi.Application.Interfaces;
using SampleApi.Application.Services;
using SampleApi.Infrastruture.Interfaces;

namespace SampleApiTests
{
    public class CustomerServiceTests
    {
        private static readonly Guid CustomerId = Guid.Parse("6b56b62f-0bdf-4354-a822-9b1c257c5ec0");
        private readonly Mock<ICustomerRepository> customerRepository = new Mock<ICustomerRepository>();
        private readonly Mock<IChangeNotifier> changeNotifier = new Mock<IChangeNotifier>();
        private CustomerService CreateCustomerService() => new(customerRepository.Object, changeNotifier.Object, NullLogger<CustomerService>.Instance);


        public class GetCustomerPreferencesAsyncTests : CustomerServiceTests
        {
            [Fact]
            public async Task ReturnsCustomerPreferences_WhenCustomerExists()
            {
                // Arrange
                var expectedPreferences = new CustomerPreferencesResponse(CustomerId, true, true);
                customerRepository
                    .Setup(r => r.GetCustomerPreferencesAsync(CustomerId, CancellationToken.None))
                    .ReturnsAsync(expectedPreferences);

                // Act
                var result = await CreateCustomerService().GetCustomerPreferencesAsync(CustomerId, CancellationToken.None);

                // Assert
                Assert.Equal(expectedPreferences, result);
            }

            [Fact]
            public async Task ReturnsNull_WhenCustomerDoesNotExist()
            {
                // Arrange
                customerRepository
                    .Setup(r => r.GetCustomerPreferencesAsync(CustomerId, CancellationToken.None))
                    .ReturnsAsync((CustomerPreferencesResponse?)null);

                // Act
                var result = await CreateCustomerService().GetCustomerPreferencesAsync(CustomerId, CancellationToken.None);

                // Assert
                Assert.Null(result);
            }
        }

        public class UpdateCustomerPreferencesAsyncTests : CustomerServiceTests
        {
            [Fact]
            public async Task ReturnsNotFound_WhenCustomerDoesNotExist()
            {
                // Arrange
                var request = new CustomerPreferences(CustomerId, true, true);
                customerRepository
                    .Setup(r => r.UpdateCustomerPreferencesAsync(request, CancellationToken.None))
                    .ReturnsAsync(UpdatePreferencesResult.CustomerNotFound);

                // Act
                var result = await CreateCustomerService().UpdateCustomerPreferencesAsync(request, CancellationToken.None);

                // Assert
                Assert.True(result == UpdateOperationResult.NotFound);
                customerRepository.Verify(r => r.UpdateCustomerPreferencesAsync(request, CancellationToken.None), Times.Once);
            }

            [Fact]
            public async Task ReturnsFailure_WhenNotificationFails()
            {
                // Arrange
                var request = new CustomerPreferences(CustomerId, true, true);
                customerRepository
                    .Setup(r => r.UpdateCustomerPreferencesAsync(request, CancellationToken.None))
                    .ReturnsAsync(UpdatePreferencesResult.Updated);
                changeNotifier
                    .Setup(n => n.NotifyCustomerPreferencesUpdateAsync(CustomerId, true, true, CancellationToken.None))
                    .ThrowsAsync(new Exception("Notification failed"));

                // Act
                var result = await CreateCustomerService().UpdateCustomerPreferencesAsync(request, CancellationToken.None);

                // Assert
                Assert.True(result == UpdateOperationResult.Failure);
                customerRepository.Verify(r => r.UpdateCustomerPreferencesAsync(request, CancellationToken.None), Times.Once);
                changeNotifier.Verify(n => n.NotifyCustomerPreferencesUpdateAsync(CustomerId, true, true, CancellationToken.None), Times.Once);
            }

            [Fact]
            public async Task ReturnsSuccess_WhenCustomerExistsAndUpdatedAndNotified()
            {
                // Arrange
                var request = new CustomerPreferences(CustomerId, true, true);
                customerRepository
                    .Setup(r => r.UpdateCustomerPreferencesAsync(new CustomerPreferences(CustomerId, true, true), CancellationToken.None))
                    .ReturnsAsync(UpdatePreferencesResult.Updated);

                changeNotifier
                    .Setup(n => n.NotifyCustomerPreferencesUpdateAsync(CustomerId, true, true, CancellationToken.None))
                    .ReturnsAsync(true);

                // Act
                var result = await CreateCustomerService().UpdateCustomerPreferencesAsync(request, CancellationToken.None);

                // Assert
                Assert.True(result == UpdateOperationResult.Success);
                customerRepository.Verify(r => r.UpdateCustomerPreferencesAsync(request, CancellationToken.None), Times.Once);
                changeNotifier.Verify(n => n.NotifyCustomerPreferencesUpdateAsync(CustomerId, true, true, CancellationToken.None), Times.Once);
            }

            [Fact]
            public async Task ReturnsSuccess_WhenCustomerExistsAndUnChanged()
            {
                // Arrange
                var request = new CustomerPreferences(CustomerId, true, true);
                customerRepository
                    .Setup(r => r.UpdateCustomerPreferencesAsync(new CustomerPreferences(CustomerId, true, true), CancellationToken.None))
                    .ReturnsAsync(UpdatePreferencesResult.UnChanged);


                // Act
                var result = await CreateCustomerService().UpdateCustomerPreferencesAsync(request, CancellationToken.None);

                // Assert
                Assert.True(result == UpdateOperationResult.Success);
                customerRepository.Verify(r => r.UpdateCustomerPreferencesAsync(request, CancellationToken.None), Times.Once);
                changeNotifier.VerifyNoOtherCalls();
            }
        }
    }
}
