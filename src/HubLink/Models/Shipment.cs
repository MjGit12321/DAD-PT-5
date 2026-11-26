using System;
using System.Globalization;

namespace HubLink.Models;

/// <summary>
/// Represents a shipment record retrieved from the central HubLink service.
/// </summary>
public sealed class Shipment
{
    public required string TrackingNumber { get; init; }
    public required string BranchCode { get; init; }
    public required string Origin { get; init; }
    public required string Destination { get; init; }
    public required string Status { get; init; }
    public required string LastUpdatedUtc { get; init; }

    public string LastUpdatedDisplay
    {
        get
        {
            if (DateTimeOffset.TryParse(
                LastUpdatedUtc,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                out var parsed))
            {
                return parsed.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
            }

            return LastUpdatedUtc;
        }
    }
}
