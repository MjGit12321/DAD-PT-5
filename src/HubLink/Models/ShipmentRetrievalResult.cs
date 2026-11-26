using System;
using System.Collections.Generic;

namespace HubLink.Models;

/// <summary>
/// Represents the outcome of an external shipment retrieval request.
/// </summary>
public sealed class ShipmentRetrievalResult
{
    private ShipmentRetrievalResult(
        bool isSuccess,
        string branchCode,
        IReadOnlyList<Shipment> shipments,
        string? errorMessage,
        DateTimeOffset? retrievedAtUtc)
    {
        IsSuccess = isSuccess;
        BranchCode = branchCode;
        Shipments = shipments;
        ErrorMessage = errorMessage;
        RetrievedAtUtc = retrievedAtUtc;
    }

    public bool IsSuccess { get; }
    public string BranchCode { get; }
    public IReadOnlyList<Shipment> Shipments { get; }
    public string? ErrorMessage { get; }
    public DateTimeOffset? RetrievedAtUtc { get; }

    public static ShipmentRetrievalResult Success(string branchCode, IReadOnlyList<Shipment> shipments) =>
        new(true, branchCode, shipments, null, DateTimeOffset.UtcNow);

    public static ShipmentRetrievalResult Failure(string branchCode, string errorMessage) =>
        new(false, branchCode, Array.Empty<Shipment>(), errorMessage, null);
}
