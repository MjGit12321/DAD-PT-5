using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using HubLink.Models;

namespace HubLink.Services;

/// <summary>
/// Retrieves and validates shipment data from the central HubLink REST API.
/// </summary>
public sealed class ShipmentApiService : IShipmentApiService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient _httpClient;

    public ShipmentApiService(HttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    public async Task<ShipmentRetrievalResult> GetShipmentsAsync(
        HubLinkSettings settings,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(settings);

        var branchCode = settings.BranchCode?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(branchCode))
        {
            return ShipmentRetrievalResult.Failure(branchCode, "Branch code is required (for example, BR-001).");
        }

        if (!TryBuildRequestUri(settings, branchCode, out var requestUri, out var uriError))
        {
            return ShipmentRetrievalResult.Failure(branchCode, uriError!);
        }

        var timeoutSeconds = Math.Clamp(settings.TimeoutSeconds, 1, 120);
        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(TimeSpan.FromSeconds(timeoutSeconds));

        try
        {
            using var response = await _httpClient.GetAsync(requestUri, timeoutCts.Token);
            var body = await response.Content.ReadAsStringAsync(timeoutCts.Token);

            if (!response.IsSuccessStatusCode)
            {
                var apiError = TryExtractErrorMessage(body);
                var statusText = $"HTTP {(int)response.StatusCode} ({response.ReasonPhrase})";
                var message = string.IsNullOrWhiteSpace(apiError)
                    ? $"The service returned an unsuccessful response: {statusText}."
                    : $"The service returned {statusText}: {apiError}";

                return ShipmentRetrievalResult.Failure(branchCode, message);
            }

            return ParseShipmentResponse(body, branchCode);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested && timeoutCts.IsCancellationRequested)
        {
            return ShipmentRetrievalResult.Failure(
                branchCode,
                $"The request timed out after {timeoutSeconds} second(s). Check that the service is responsive and try again.");
        }
        catch (OperationCanceledException)
        {
            return ShipmentRetrievalResult.Failure(branchCode, "The request was cancelled.");
        }
        catch (HttpRequestException ex)
        {
            return ShipmentRetrievalResult.Failure(
                branchCode,
                $"Unable to reach the HubLink service at {settings.BaseUrl}. ({ex.Message})");
        }
        catch (Exception ex)
        {
            return ShipmentRetrievalResult.Failure(
                branchCode,
                $"An unexpected error occurred while retrieving shipments: {ex.Message}");
        }
    }

    private static bool TryBuildRequestUri(
        HubLinkSettings settings,
        string branchCode,
        out Uri? uri,
        out string? error)
    {
        uri = null;
        error = null;

        var baseUrl = settings.BaseUrl?.Trim().TrimEnd('/') ?? string.Empty;
        if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out var baseUri) ||
            (baseUri.Scheme != Uri.UriSchemeHttp && baseUri.Scheme != Uri.UriSchemeHttps))
        {
            error = "Service Base URL must be a valid HTTP or HTTPS address (for example, http://127.0.0.1:5080).";
            return false;
        }

        var endpointPath = string.IsNullOrWhiteSpace(settings.EndpointPath)
            ? "/api/shipments"
            : settings.EndpointPath.Trim();

        if (!endpointPath.StartsWith('/'))
        {
            endpointPath = "/" + endpointPath;
        }

        var query = $"?branchCode={Uri.EscapeDataString(branchCode)}";
        if (!string.IsNullOrWhiteSpace(settings.Mode))
        {
            query += $"&mode={Uri.EscapeDataString(settings.Mode.Trim())}";
        }

        uri = new Uri($"{baseUrl}{endpointPath}{query}", UriKind.Absolute);
        return true;
    }

    private static ShipmentRetrievalResult ParseShipmentResponse(string json, string requestedBranch)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return ShipmentRetrievalResult.Failure(
                requestedBranch,
                "The service returned an empty response body instead of valid JSON.");
        }

        ShipmentApiResponseDto? dto;
        try
        {
            dto = JsonSerializer.Deserialize<ShipmentApiResponseDto>(json, JsonOptions);
        }
        catch (JsonException ex)
        {
            return ShipmentRetrievalResult.Failure(
                requestedBranch,
                $"The service returned invalid JSON that could not be parsed. ({ex.Message})");
        }

        if (dto is null || string.IsNullOrWhiteSpace(dto.BranchCode) || dto.Shipments is null)
        {
            return ShipmentRetrievalResult.Failure(
                requestedBranch,
                "The JSON response is missing required fields (branchCode or shipments).");
        }

        var shipments = new List<Shipment>(dto.Shipments.Count);
        foreach (var item in dto.Shipments)
        {
            if (item is null ||
                string.IsNullOrWhiteSpace(item.TrackingNumber) ||
                string.IsNullOrWhiteSpace(item.Origin) ||
                string.IsNullOrWhiteSpace(item.Destination) ||
                string.IsNullOrWhiteSpace(item.Status) ||
                string.IsNullOrWhiteSpace(item.LastUpdatedUtc))
            {
                return ShipmentRetrievalResult.Failure(
                    requestedBranch,
                    "The response contained an incomplete shipment record with missing required fields.");
            }

            shipments.Add(new Shipment
            {
                TrackingNumber = item.TrackingNumber,
                BranchCode = string.IsNullOrWhiteSpace(item.BranchCode) ? dto.BranchCode : item.BranchCode,
                Origin = item.Origin,
                Destination = item.Destination,
                Status = item.Status,
                LastUpdatedUtc = item.LastUpdatedUtc
            });
        }

        return ShipmentRetrievalResult.Success(dto.BranchCode, shipments);
    }

    private static string? TryExtractErrorMessage(string body)
    {
        if (string.IsNullOrWhiteSpace(body))
        {
            return null;
        }

        try
        {
            var errorDto = JsonSerializer.Deserialize<ErrorResponseDto>(body, JsonOptions);
            return errorDto?.Error;
        }
        catch
        {
            return null;
        }
    }

    private sealed class ShipmentApiResponseDto
    {
        public string? BranchCode { get; set; }
        public List<ShipmentItemDto?>? Shipments { get; set; }
    }

    private sealed class ShipmentItemDto
    {
        public string? TrackingNumber { get; set; }
        public string? BranchCode { get; set; }
        public string? Origin { get; set; }
        public string? Destination { get; set; }
        public string? Status { get; set; }
        public string? LastUpdatedUtc { get; set; }
    }

    private sealed class ErrorResponseDto
    {
        public string? Error { get; set; }
    }
}
