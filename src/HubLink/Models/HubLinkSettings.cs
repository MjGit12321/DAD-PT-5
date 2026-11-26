namespace HubLink.Models;

/// <summary>
/// Configuration settings for connecting to the HubLink REST API.
/// </summary>
public sealed class HubLinkSettings
{
    public string BaseUrl { get; set; } = "http://127.0.0.1:5080";
    public string EndpointPath { get; set; } = "/api/shipments";
    public string BranchCode { get; set; } = "BR-001";
    public int TimeoutSeconds { get; set; } = 5;
    public string Mode { get; set; } = string.Empty;
}
