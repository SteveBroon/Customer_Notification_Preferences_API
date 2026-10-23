using System.Net;
using System.Net.Http.Json;
using System.Text;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SampleApi.Application.DTO;
using SampleApi.Infrastruture;
using SampleApi.Infrastruture.Interfaces;

namespace SampleApiTests
{
    public class IntegrationTests
    {
        private static readonly Guid ExistingCustomerId = Guid.Parse("6b56b62f-0bdf-4354-a822-9b1c257c5ec0");

        // AC01
        [Fact]
        public async Task GetExistingCustomer_ReturnsPreferences()
        {
            // Arrange
            using var factory = new CustomerApiFactory();
            using var client = factory.CreateClient();

            // Act
            var response = await client.GetAsync(
                $"/api/customers/{ExistingCustomerId}/notification-preferences");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var preferences = await response.Content.ReadFromJsonAsync<CustomerPreferencesResponse>();
            Assert.Equal(new CustomerPreferencesResponse(ExistingCustomerId, true, false), preferences);
        }

        // AC02
        [Fact]
        public async Task Get_UnknownCustomer_ReturnsNotFound()
        {
            // Arrange
            using var factory = new CustomerApiFactory();
            using var client = factory.CreateClient();

            // Act
            var response = await client.GetAsync(
                $"/api/customers/{Guid.Parse("00000000-0000-0000-0000-000000000000")}/notification-preferences");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        // AC09
        [Fact]
        public async Task Get_InvalidCustomerId_ReturnsBadRequest()
        {
            // Arrange
            using var factory = new CustomerApiFactory();
            using var client = factory.CreateClient();

            // Act
            var response = await client.GetAsync(
                "/api/customers/not-a-guid/notification-preferences");

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Theory]
        [InlineData("6b56b62f-0bdf-4354-a822-9b1c257c5ec0", true, false)]
        [InlineData("af285565-031a-4df2-9f82-98f101dd08fa", false, true)]
        [InlineData("abb0125e-b3ed-4a66-910b-492f431f9486", true, true)]
        [InlineData("e2906422-3298-4b81-8a78-39544d94268e", false, false)]
        [InlineData("6d4977a7-a9a4-4b72-b75d-00569f699602", true, false)]
        [InlineData("de5de5f6-0d9d-472d-98be-b45db46f84ca", false, true)]
        public async Task SeededCustomer_ReturnsExpectedPreferences(string customerId, bool emailEnabled, bool smsEnabled)
        {
            // Arrange
            using var factory = new CustomerApiFactory();
            using var client = factory.CreateClient();
            var id = Guid.Parse(customerId);

            // Act
            var response = await client.GetAsync(
                $"/api/customers/{id}/notification-preferences");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var preferences = await response.Content.ReadFromJsonAsync<CustomerPreferencesResponse>();
            Assert.Equal(new CustomerPreferencesResponse(id, emailEnabled, smsEnabled), preferences);
        }
        
        // AC03
        [Fact]
        public async Task Put_ChangedPreferences_ReturnsOkWithUpdatedValues()
        {
            // Arrange
            using var factory = new CustomerApiFactory();
            using var client = factory.CreateClient();

            // Act
            var response = await client.PutAsJsonAsync(
                $"/api/customers/{ExistingCustomerId}/notification-preferences",
                new CustomerPreferencesRequest(false, true));

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var updated = await response.Content.ReadFromJsonAsync<CustomerPreferencesResponse>();
            Assert.Equal(new CustomerPreferencesResponse(ExistingCustomerId, false, true), updated);
        }

        // AC04
        [Fact]
        public async Task Get_AfterSuccessfulPut_ReturnsUpdatedValues()
        {
            // Arrange
            using var factory = new CustomerApiFactory();
            using var client = factory.CreateClient();
            await client.PutAsJsonAsync(
                $"/api/customers/{ExistingCustomerId}/notification-preferences",
                new CustomerPreferencesRequest(false, true));

            // Act
            var response = await client.GetAsync(
                $"/api/customers/{ExistingCustomerId}/notification-preferences");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var persisted = await response.Content.ReadFromJsonAsync<CustomerPreferencesResponse>();
            Assert.Equal(new CustomerPreferencesResponse(ExistingCustomerId, false, true), persisted);
        }
        
        // AC05
        [Fact]
        public async Task Put_UnchangedPreferences_DoesNotCallNotifier()
        {
            // Arrange
            using var factory = new CustomerApiFactory();
            using var client = factory.CreateClient();


            var response = await client.PutAsJsonAsync(
                $"/api/customers/{ExistingCustomerId}/notification-preferences",
                new CustomerPreferencesRequest(true, false));

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Empty(factory.Notifier.Notifications);
        }

        // AC06
        [Fact]
        public async Task Put_ChangedPreferences_CallsNotifierOnceWithUpdatedValues()
        {
            // Arrange
            using var factory = new CustomerApiFactory();
            using var client = factory.CreateClient();

            // Act
            await client.PutAsJsonAsync(
                $"/api/customers/{ExistingCustomerId}/notification-preferences",
                new CustomerPreferencesRequest(false, true));

            // Assert
            Assert.Equal(
                new[] { new Notification(ExistingCustomerId, false, true) },
                factory.Notifier.Notifications);
        }

        // AC07
        [Fact]
        public async Task Put_UnknownCustomer_ReturnsNotFound_DoesNotCreateCustomer_OrCallNotifier()
        {
            // Arrange
            using var factory = new CustomerApiFactory();
            using var client = factory.CreateClient();
            var unknownCustomerId = Guid.NewGuid();

            // Act
            var response = await client.PutAsJsonAsync(
                $"/api/customers/{unknownCustomerId}/notification-preferences",
                new CustomerPreferencesRequest(false, true));

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            var getResponse = await client.GetAsync(
                $"/api/customers/{unknownCustomerId}/notification-preferences");
            Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
            Assert.Empty(factory.Notifier.Notifications);
        }

        // AC08
        [Theory]
        [InlineData("", null, null)]
        [InlineData("{", null, null)]
        [InlineData("""{"smsEnabled":true}""", "EmailEnabled", "Email preference is required.")]
        [InlineData("""{"emailEnabled":true}""", "SmsEnabled", "SMS preference is required.")]
        public async Task Put_InvalidBody_ReturnsBadRequest_DoesNotChangePreferences_OrNotify(
            string body, string? expectedProperty, string? expectedMessage)
        {
            // Arrange
            using var factory = new CustomerApiFactory();
            using var client = factory.CreateClient();
            using var content = new StringContent(body, Encoding.UTF8, "application/json");

            // Act
            var response = await client.PutAsync(
                $"/api/customers/{ExistingCustomerId}/notification-preferences",
                content);
            var getResponse = await client.GetAsync(
                $"/api/customers/{ExistingCustomerId}/notification-preferences");

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Empty(factory.Notifier.Notifications);

            var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
            Assert.NotNull(problem);
            Assert.Equal(400, problem.Status);
            Assert.NotEmpty(problem.Errors);
            if (expectedProperty is not null)
            {
                Assert.Contains(expectedMessage, problem.Errors[expectedProperty]);
            }

            Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
            var persisted = await getResponse.Content.ReadFromJsonAsync<CustomerPreferencesResponse>();
            Assert.Equal(new CustomerPreferencesResponse(ExistingCustomerId, true, false), persisted);
        }

        // AC09
        [Fact]
        public async Task Put_InvalidCustomerId_ReturnsBadRequest_AndDoesNotNotify()
        {
            // Arrange
            using var factory = new CustomerApiFactory();
            using var client = factory.CreateClient();

            // Act
            var response = await client.PutAsJsonAsync(
                "/api/customers/not-a-guid/notification-preferences",
                new CustomerPreferencesRequest(false, true));
            
            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Empty(factory.Notifier.Notifications);
        }

        // AC10
        [Fact]
        public async Task Put_NotificationFailure_ReturnsServiceUnavailable_AndKeepsPreferencesUpdated()
        {
            // Arrange
            using var factory = new CustomerApiFactory();
            factory.Notifier.ThrowOnNotify = true;
            using var client = factory.CreateClient();

            // Act
            var response = await client.PutAsJsonAsync(
                $"/api/customers/{ExistingCustomerId}/notification-preferences",
                new CustomerPreferencesRequest(false, true));

            // Assert
            Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
            var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
            Assert.NotNull(problem);
            Assert.Equal(503, problem.Status);
            Assert.Equal("Notification service unavailable", problem.Title);
            Assert.Equal("Preferences were saved, but the notification system could not be informed.", problem.Detail);

            var getResponse = await client.GetAsync(
                $"/api/customers/{ExistingCustomerId}/notification-preferences");
            Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
            var persisted = await getResponse.Content.ReadFromJsonAsync<CustomerPreferencesResponse>();
            Assert.Equal(new CustomerPreferencesResponse(ExistingCustomerId, false, true), persisted);
            Assert.Equal(
                new[] { new Notification(ExistingCustomerId, false, true) },
                factory.Notifier.Notifications);
        }

        private class CustomerApiFactory : WebApplicationFactory<Program>
        {
            private string DatabaseName { get; } = $"CustomerDatabase-{Guid.NewGuid()}";

            public TestChangeNotifier Notifier { get; } = new();

            protected override void ConfigureWebHost(IWebHostBuilder builder)
            {
                builder.ConfigureTestServices(services =>
                {
                    services.RemoveAll<DbContextOptions<CustomerDbContext>>();
                    services.RemoveAll<CustomerDbContext>();
                    services.AddDbContext<CustomerDbContext>(options =>
                        options.UseInMemoryDatabase(DatabaseName));

                    services.RemoveAll<IChangeNotifier>();
                    services.AddSingleton(Notifier);
                    services.AddSingleton<IChangeNotifier>(provider =>
                        provider.GetRequiredService<TestChangeNotifier>());
                });
            }
        }

        private class TestChangeNotifier : IChangeNotifier
        {
            public List<Notification> Notifications { get; } = [];
            public bool ThrowOnNotify { get; set; } = false;
    
            public void NotifyChange() =>
                        Notifications.Add(new Notification(Guid.Empty, false, false));

            public Task<bool> NotifyCustomerPreferencesUpdateAsync(
                Guid customerId,
                bool emailEnabled,
                bool smsEnabled,
                CancellationToken cancellationToken)
            {
                Notifications.Add(new Notification(customerId, emailEnabled, smsEnabled));
                if (ThrowOnNotify)
                {
                    throw new InvalidOperationException("Notification failed.");
                }

                return Task.FromResult(true);
            }
        }

        private record Notification(Guid CustomerId, bool EmailEnabled, bool SmsEnabled);

    }
}
