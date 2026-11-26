using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HubLink.Models;
using HubLink.Services;

namespace HubLink.Tests;

public sealed class ShipmentApiServiceTests
{
    [Fact]
    public async Task GetShipmentsAsync_ValidJson_ReturnsParsedShipments()
    {
        const string json = """
        {
          "branchCode": "BR-001",
          "shipments": [
            {
              "trackingNumber": "HUB-1001",
              "branchCode": "BR-001",
              "origin": "Cagayan de Oro",
              "destination": "Valencia",
              "status": "InTransit",
              "lastUpdatedUtc": "2026-11-10T08:30:00Z"
            }
          ]
        }
        """;

        var service = CreateService(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        });

        var result = await service.GetShipmentsAsync(new HubLinkSettings { BranchCode = "BR-001" });

        Assert.True(result.IsSuccess);
        Assert.Equal("BR-001", result.BranchCode);
        Assert.Single(result.Shipments);
        Assert.Equal("HUB-1001", result.Shipments[0].TrackingNumber);
    }

    [Fact]
    public async Task GetShipmentsAsync_EmptyShipmentsArray_ReturnsSuccessWithZeroItems()
    {
        const string json = """{"branchCode":"BR-999","shipments":[]}""";
        var service = CreateService(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        });

        var result = await service.GetShipmentsAsync(new HubLinkSettings { BranchCode = "BR-999" });

        Assert.True(result.IsSuccess);
        Assert.Equal("BR-999", result.BranchCode);
        Assert.Empty(result.Shipments);
    }

    [Theory]
    [InlineData(HttpStatusCode.BadRequest, "{\"error\":\"Simulated bad request.\"}", "Simulated bad request.")]
    [InlineData(HttpStatusCode.InternalServerError, "{\"error\":\"Simulated server error.\"}", "Simulated server error.")]
    public async Task GetShipmentsAsync_HttpError_ReturnsFailureWithMessage(
        HttpStatusCode statusCode,
        string body,
        string expectedSubstring)
    {
        var service = CreateService(_ => new HttpResponseMessage(statusCode)
        {
            Content = new StringContent(body, Encoding.UTF8, "application/json")
        });

        var result = await service.GetShipmentsAsync(new HubLinkSettings { BranchCode = "BR-001" });

        Assert.False(result.IsSuccess);
        Assert.NotNull(result.ErrorMessage);
        Assert.Contains(expectedSubstring, result.ErrorMessage);
    }

    [Fact]
    public async Task GetShipmentsAsync_InvalidJson_ReturnsFailure()
    {
        var service = CreateService(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("{\"branchCode\":", Encoding.UTF8, "application/json")
        });

        var result = await service.GetShipmentsAsync(new HubLinkSettings { BranchCode = "BR-001" });

        Assert.False(result.IsSuccess);
        Assert.Contains("invalid JSON", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GetShipmentsAsync_MissingRequiredShipmentFields_ReturnsFailure()
    {
        const string json = """
        {
          "branchCode": "BR-001",
          "shipments": [
            { "trackingNumber": "HUB-1001" }
          ]
        }
        """;

        var service = CreateService(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        });

        var result = await service.GetShipmentsAsync(new HubLinkSettings { BranchCode = "BR-001" });

        Assert.False(result.IsSuccess);
        Assert.Contains("missing required fields", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GetShipmentsAsync_SlowResponse_TriggersTimeoutFailure()
    {
        var service = CreateAsyncService(async ct =>
        {
            await Task.Delay(TimeSpan.FromSeconds(5), ct);
            return new HttpResponseMessage(HttpStatusCode.OK);
        });

        var result = await service.GetShipmentsAsync(new HubLinkSettings
        {
            BranchCode = "BR-001",
            TimeoutSeconds = 1
        });

        Assert.False(result.IsSuccess);
        Assert.Contains("timed out", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GetShipmentsAsync_NetworkFailure_ReturnsFailure()
    {
        var service = CreateService(_ => throw new HttpRequestException("Connection refused"));

        var result = await service.GetShipmentsAsync(new HubLinkSettings { BranchCode = "BR-001" });

        Assert.False(result.IsSuccess);
        Assert.Contains("Unable to reach", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }

    private static ShipmentApiService CreateAsyncService(Func<CancellationToken, Task<HttpResponseMessage>> handler)
    {
        var httpClient = new HttpClient(new StubMessageHandler(handler));
        return new ShipmentApiService(httpClient);
    }

    private static ShipmentApiService CreateService(Func<HttpRequestMessage, HttpResponseMessage> handler)
    {
        var httpClient = new HttpClient(new StubMessageHandler(req => Task.FromResult(handler(req))));
        return new ShipmentApiService(httpClient);
    }

    private sealed class StubMessageHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> _handler;

        public StubMessageHandler(Func<HttpRequestMessage, Task<HttpResponseMessage>> handler)
        {
            _handler = (req, _) => handler(req);
        }

        public StubMessageHandler(Func<CancellationToken, Task<HttpResponseMessage>> handler)
        {
            _handler = (_, ct) => handler(ct);
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => _handler(request, cancellationToken);
    }
}
