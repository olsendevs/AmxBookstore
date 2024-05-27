using System.Net.Http.Json;
using Xunit;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Collections.Generic;
using System.Linq;
using Moq;

public class HealthCheckControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly WebApplicationFactory<Program> _factory;

    public HealthCheckControllerTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Mock the HealthCheckService
                var mockHealthCheckService = new Mock<HealthCheckService>();

                // Setup mock behavior for CheckHealthAsync
                var healthReportEntries = new Dictionary<string, HealthReportEntry>
                {
                    { "TestComponent", new HealthReportEntry(HealthStatus.Healthy, "Healthy", TimeSpan.Zero, null, null) }
                };
                var healthReport = new HealthReport(healthReportEntries, TimeSpan.Zero);

                mockHealthCheckService.Setup(service => service.CheckHealthAsync(It.IsAny<Func<HealthCheckRegistration, bool>>(), It.IsAny<CancellationToken>()))
                                      .ReturnsAsync(healthReport);

                services.AddSingleton(mockHealthCheckService.Object);
            });
        });
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task Get_ShouldReturnHealthyStatus_WhenHealthChecksPass()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/HealthCheck");

        // Assert
        response.EnsureSuccessStatusCode();
        var healthStatus = await response.Content.ReadFromJsonAsync<HealthStatusResponse>();
        Assert.NotNull(healthStatus);
        Assert.Equal("Healthy", healthStatus.Status);
        Assert.NotEmpty(healthStatus.Results);
        Assert.Equal("Healthy", healthStatus.Results.First().Status);
        Assert.Equal("TestComponent", healthStatus.Results.First().Name);
    }

    public class HealthStatusResponse
    {
        public string Status { get; set; }
        public List<HealthCheckResult> Results { get; set; }
    }

    public class HealthCheckResult
    {
        public string Name { get; set; }
        public string Status { get; set; }
        public string Description { get; set; }
    }
}
