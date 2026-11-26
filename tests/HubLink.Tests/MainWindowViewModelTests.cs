using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using HubLink.Models;
using HubLink.Services;
using HubLink.ViewModels;

namespace HubLink.Tests;

public sealed class MainWindowViewModelTests
{
    [Fact]
    public async Task RefreshFilterAndSummary_BehaveCorrectlyAndExportFilteredOnly()
    {
        var fakeApi = new FakeShipmentApiService(
            ShipmentRetrievalResult.Success("BR-001",
            [
                MakeShipment("HUB-1001", "InTransit"),
                MakeShipment("HUB-1002", "Pending"),
                MakeShipment("HUB-1003", "Delivered"),
                MakeShipment("HUB-1004", "Exception"),
                MakeShipment("HUB-1005", "InTransit")
            ]));

        var vm = new MainWindowViewModel(fakeApi, new CsvExportService(), new HubLinkSettings());

        await vm.RefreshShipmentsAsync();

        Assert.False(vm.HasError);
        Assert.Equal(5, vm.Summary.TotalCount);
        Assert.Equal(1, vm.Summary.PendingCount);
        Assert.Equal(2, vm.Summary.InTransitCount);
        Assert.Equal(1, vm.Summary.DeliveredCount);
        Assert.Equal(1, vm.Summary.ExceptionCount);
        Assert.Equal(5, vm.FilteredShipments.Count);

        // Filter to InTransit (in-memory, no extra API call)
        vm.SelectedStatusFilter = "InTransit";
        Assert.Equal(2, vm.FilteredShipments.Count);
        Assert.Equal(5, vm.Summary.TotalCount);
        Assert.Equal(1, fakeApi.CallCount);

        // Export while filtered
        var tempFile = Path.Combine(Path.GetTempPath(), $"hublink-vm-{Guid.NewGuid():N}.csv");
        try
        {
            var exportResult = await vm.ExportCsvToPathAsync(tempFile);
            Assert.True(exportResult.IsSuccess);
            Assert.Equal(2, exportResult.ExportedCount);

            var lines = await File.ReadAllLinesAsync(tempFile);
            Assert.Equal(3, lines.Length); // header + 2 InTransit rows
        }
        finally
        {
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }
        }

        // Restore All
        vm.SelectedStatusFilter = "All";
        Assert.Equal(5, vm.FilteredShipments.Count);
    }

    [Fact]
    public async Task RefreshFailureAfterSuccess_MarksStaleDataAndPreservesPreviousShipments()
    {
        var fakeApi = new FakeShipmentApiService(
            ShipmentRetrievalResult.Success("BR-001", [MakeShipment("HUB-1001", "InTransit")]),
            ShipmentRetrievalResult.Failure("BR-001", "Simulated server error."));

        var vm = new MainWindowViewModel(fakeApi, new CsvExportService(), new HubLinkSettings());

        await vm.RefreshShipmentsAsync();
        Assert.False(vm.IsStaleData);
        Assert.Single(vm.FilteredShipments);

        await vm.RefreshShipmentsAsync();
        Assert.True(vm.HasError);
        Assert.True(vm.IsStaleData);
        Assert.Single(vm.FilteredShipments);
    }

    private static Shipment MakeShipment(string tracking, string status) => new()
    {
        TrackingNumber = tracking,
        BranchCode = "BR-001",
        Origin = "Cagayan de Oro",
        Destination = "Valencia",
        Status = status,
        LastUpdatedUtc = "2026-11-10T08:30:00Z"
    };

    private sealed class FakeShipmentApiService : IShipmentApiService
    {
        private readonly Queue<ShipmentRetrievalResult> _results;
        public int CallCount { get; private set; }

        public FakeShipmentApiService(params ShipmentRetrievalResult[] results)
        {
            _results = new Queue<ShipmentRetrievalResult>(results);
        }

        public Task<ShipmentRetrievalResult> GetShipmentsAsync(HubLinkSettings settings, CancellationToken cancellationToken = default)
        {
            CallCount++;
            return Task.FromResult(_results.Dequeue());
        }
    }
}
