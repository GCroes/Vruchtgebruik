using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;
using Microsoft.AspNetCore.Mvc.Testing;
using Vruchtgebruik.Api.Models;
using Xunit.Abstractions;

namespace Vruchtgebruik.IntegrationTests
{
    public class CalculateControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly ITestOutputHelper _testOutputHelper;

        public CalculateControllerIntegrationTests(WebApplicationFactory<Program> factory, ITestOutputHelper testOutputHelper)
        {
            _factory = factory;
            _testOutputHelper = testOutputHelper;
        }

        [Fact]
        public async Task Calculate_Returns200AndExpectedResponse()
        {
            // Arrange
            var client = _factory.CreateClient();
            var request = new CalculationRequest
            {
                AssetValue = 1000,
                Age = 35,
                Sex = "man",
                FactorMethod = "EenLeven"
            };

            // Act
            var response = await client.PostAsJsonAsync("/api/calculate", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var contentString = await response.Content.ReadAsStringAsync();
            _testOutputHelper.WriteLine(contentString);

            var result = await response.Content.ReadFromJsonAsync<CalculationApiResponse>();

            result.Should().NotBeNull();
            result.Response.AssetValue.Should().Be(1000);
            result.Response.UsageValue.Should().BeGreaterThan(0);

            // CorrelationId header
            response.Headers.Should().ContainKey("X-Correlation-Id");
        }

        [Fact]
        public async Task Calculate_InvalidModel_Returns400AndProblemDetails()
        {
            var client = _factory.CreateClient();

            var request = new CalculationRequest
            {
                AssetValue = -100, // Invalid
                Age = 35,
                Sex = "man",
                FactorMethod = "EenLeven"
            };

            var response = await client.PostAsJsonAsync("/api/calculate", request);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var content = await response.Content.ReadAsStringAsync();
            content.Should().Contain("Validation failed"); // From ProblemDetails.Title
        }

        [Fact]
        public async Task NotFoundEndpoint_Returns404WithProblemDetails()
        {
            var client = _factory.CreateClient();

            var response = await client.GetAsync("/api/unknownendpoint");
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);

            var content = await response.Content.ReadAsStringAsync();
            content.Should().Contain("Resource Not Found"); // From your NotFoundHandler
        }

        [Fact]
        public async Task ExceptionHandler_Returns500WithProblemDetails()
        {
            var client = _factory.CreateClient();

            // POST with a FactorMethod that doesn't exist triggers ArgumentException → 400,
            // But you can add a dedicated test endpoint/controller that throws Exception for 500, e.g.:
            var response = await client.GetAsync("/api/calculate/throw");

            response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
            var content = await response.Content.ReadAsStringAsync();
            content.Should().Contain("An unexpected error occurred.");
        }
    }
}