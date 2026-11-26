using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HubLink.Models;

namespace HubLink.Services;

/// <summary>
/// Exports shipment records to an RFC 4180 compliant CSV file.
/// </summary>
public sealed class CsvExportService : ICsvExportService
{
    private const string HeaderRow = "TrackingNumber,Origin,Destination,Status,LastUpdatedUtc";

    public async Task<CsvExportResult> ExportAsync(
        string filePath,
        IReadOnlyList<Shipment> shipments,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            return CsvExportResult.Failure("No destination file path was specified.");
        }

        if (shipments is null || shipments.Count == 0)
        {
            return CsvExportResult.Failure("No shipment records are currently displayed to export.");
        }

        try
        {
            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrWhiteSpace(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var content = BuildCsvContent(shipments);
            await File.WriteAllTextAsync(filePath, content, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false), cancellationToken);

            return CsvExportResult.Success(filePath, shipments.Count);
        }
        catch (OperationCanceledException)
        {
            return CsvExportResult.Failure("CSV export was cancelled.");
        }
        catch (Exception ex)
        {
            return CsvExportResult.Failure($"Failed to export CSV file: {ex.Message}");
        }
    }

    public static string BuildCsvContent(IReadOnlyList<Shipment> shipments)
    {
        var sb = new StringBuilder();
        sb.AppendLine(HeaderRow);

        foreach (var s in shipments)
        {
            sb.Append(EscapeCsvField(s.TrackingNumber)).Append(',');
            sb.Append(EscapeCsvField(s.Origin)).Append(',');
            sb.Append(EscapeCsvField(s.Destination)).Append(',');
            sb.Append(EscapeCsvField(s.Status)).Append(',');
            sb.AppendLine(EscapeCsvField(s.LastUpdatedUtc));
        }

        return sb.ToString();
    }

    public static string EscapeCsvField(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        var requiresQuotes = value.Contains(',') ||
                             value.Contains('"') ||
                             value.Contains('\r') ||
                             value.Contains('\n');

        if (!requiresQuotes)
        {
            return value;
        }

        return $"\"{value.Replace("\"", "\"\"")}\"";
    }
}
