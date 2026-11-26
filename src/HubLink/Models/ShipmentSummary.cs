using System.Collections.Generic;
using System.Linq;

namespace HubLink.Models;

/// <summary>
/// Aggregated counts for all retrieved shipments of a branch.
/// </summary>
public sealed record ShipmentSummary(
    int TotalCount,
    int PendingCount,
    int InTransitCount,
    int DeliveredCount,
    int ExceptionCount)
{
    public static ShipmentSummary Empty { get; } = new(0, 0, 0, 0, 0);

    public static ShipmentSummary FromShipments(IReadOnlyCollection<Shipment> shipments)
    {
        if (shipments.Count == 0)
        {
            return Empty;
        }

        return new ShipmentSummary(
            TotalCount: shipments.Count,
            PendingCount: shipments.Count(s => s.Status == "Pending"),
            InTransitCount: shipments.Count(s => s.Status == "InTransit"),
            DeliveredCount: shipments.Count(s => s.Status == "Delivered"),
            ExceptionCount: shipments.Count(s => s.Status == "Exception"));
    }
}
