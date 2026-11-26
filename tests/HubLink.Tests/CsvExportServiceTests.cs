using System;
using System.IO;
using System.Threading.Tasks;
using HubLink.Models;
using HubLink.Services;

namespace HubLink.Tests;

public sealed class CsvExportServiceTests
{
    [Fact]
    public async Task ExportAsync_WritesHeaderAndEscapesCommasAndQuotes()
    {
        var tempFile = Path.Combine(Path.GetTempPath(), $"hublink-test-{Guid.NewGuid():N}.csv");
        try
        {
            var shipments = new[]
            {
                new Shipment
                {
                    TrackingNumber = "HUB-1004",
                    BranchCode = "BR-001",
                    Origin = "Butuan, Agusan del Norte",
                    Destination = "Depot \"North\"",
                    Status = "Exception",
                    LastUpdatedUtc = "2026-11-07T09:00:00Z"
                }
            };

            var service = new CsvExportService();
            var result = await service.ExportAsync(tempFile, shipments);

            Assert.True(result.IsSuccess);
            Assert.Equal(1, result.ExportedCount);

            var lines = await File.ReadAllLinesAsync(tempFile);
            Assert.Equal(2, lines.Length);
            Assert.Equal("TrackingNumber,Origin,Destination,Status,LastUpdatedUtc", lines[0]);
            Assert.Equal("HUB-1004,\"Butuan, Agusan del Norte\",\"Depot \"\"North\"\"\",Exception,2026-11-07T09:00:00Z", lines[1]);
        }
        finally
        {
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }
        }
    }

    [Fact]
    public async Task ExportAsync_EmptyShipments_ReturnsFailureWithoutCreatingFile()
    {
        var tempFile = Path.Combine(Path.GetTempPath(), $"hublink-empty-{Guid.NewGuid():N}.csv");
        var service = new CsvExportService();

        var result = await service.ExportAsync(tempFile, Array.Empty<Shipment>());

        Assert.False(result.IsSuccess);
        Assert.False(File.Exists(tempFile));
    }

    [Fact]
    public async Task ExportAsync_UnwritablePath_ReturnsFailure()
    {
        // Using an existing directory path as a file target reliably triggers an IO/UnauthorizedAccess failure.
        var dirPath = Path.GetTempPath().TrimEnd(Path.DirectorySeparatorChar);
        var shipments = new[]
        {
            new Shipment
            {
                TrackingNumber = "HUB-1001",
                BranchCode = "BR-001",
                Origin = "Cagayan de Oro",
                Destination = "Valencia",
                Status = "InTransit",
                LastUpdatedUtc = "2026-11-10T08:30:00Z"
            }
        };

        var service = new CsvExportService();
        var result = await service.ExportAsync(dirPath, shipments);

        Assert.False(result.IsSuccess);
        Assert.Contains("Failed to export CSV", result.Message);
    }
}
