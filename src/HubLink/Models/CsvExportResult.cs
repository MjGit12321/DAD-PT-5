namespace HubLink.Models;

/// <summary>
/// Represents the outcome of exporting displayed shipments to CSV.
/// </summary>
public sealed record CsvExportResult(
    bool IsSuccess,
    string Message,
    string? FilePath = null,
    int ExportedCount = 0)
{
    public static CsvExportResult Success(string filePath, int count) =>
        new(true, $"Exported {count} shipment(s) to {System.IO.Path.GetFileName(filePath)}.", filePath, count);

    public static CsvExportResult Failure(string message) =>
        new(false, message);
}
