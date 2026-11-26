using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using HubLink.Models;
using HubLink.Services;
using HubLink.ViewModels;

namespace HubLink.Tests;

public sealed class LiveApiIntegrationTests
{
    private static bool IsLiveTestEnabled() =>
        string.Equals(Environment.GetEnvironmentVariable("HUBLINK_LIVE_TEST"), "1", StringComparison.Ordinal);

    [Fact]
    public async Task VerifyAllAcceptanceScenariosAgainstLiveLocalApi()
    {
        if (!IsLiveTestEnabled())
        {
            return;
        }

        using var httpClient = new HttpClient();
        var apiService = new ShipmentApiService(httpClient);
        var csvService = new CsvExportService();
        var baseSettings = new HubLinkSettings
        {
            BaseUrl = "http://127.0.0.1:5080",
            EndpointPath = "/api/shipments",
            BranchCode = "BR-001",
            TimeoutSeconds = 3
        };

        var vm = new MainWindowViewModel(apiService, csvService, baseSettings);

        // AC-01 & AC-03: Retrieve BR-001 and verify operational summary
        await vm.RefreshShipmentsAsync();
        Assert.False(vm.HasError);
        Assert.False(vm.IsEmptyResult);
        Assert.Equal(5, vm.Summary.TotalCount);
        Assert.Equal(1, vm.Summary.PendingCount);
        Assert.Equal(2, vm.Summary.InTransitCount);
        Assert.Equal(1, vm.Summary.DeliveredCount);
        Assert.Equal(1, vm.Summary.ExceptionCount);

        // AC-02: Filter by status and restore All
        vm.SelectedStatusFilter = "InTransit";
        Assert.Equal(2, vm.FilteredShipments.Count);
        Assert.Equal(5, vm.Summary.TotalCount);
        vm.SelectedStatusFilter = "All";
        Assert.Equal(5, vm.FilteredShipments.Count);

        // AC-07: Export live BR-001 data to sample CSV file
        var samplePath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "hublink-BR-001-sample.csv"));
        var exportResult = await vm.ExportCsvToPathAsync(samplePath);
        Assert.True(exportResult.IsSuccess);
        Assert.True(File.Exists(samplePath));

        // AC-06 (valid empty): BR-999
        vm.BranchCode = "BR-999";
        await vm.RefreshShipmentsAsync();
        Assert.False(vm.HasError);
        Assert.True(vm.IsEmptyResult);
        Assert.Equal(0, vm.Summary.TotalCount);

        // AC-05: bad-request, server-error, delay timeout, unreachable port
        vm.BranchCode = "BR-001";
        vm.EndpointPath = "/api/test/shipments";

        vm.TestMode = "bad-request";
        await vm.RefreshShipmentsAsync();
        Assert.True(vm.HasError);
        Assert.Contains("400", vm.ErrorMessage);

        vm.TestMode = "server-error";
        await vm.RefreshShipmentsAsync();
        Assert.True(vm.HasError);
        Assert.Contains("500", vm.ErrorMessage);

        vm.TestMode = "delay";
        vm.TimeoutSeconds = 2;
        await vm.RefreshShipmentsAsync();
        Assert.True(vm.HasError);
        Assert.Contains("timed out", vm.ErrorMessage, StringComparison.OrdinalIgnoreCase);

        // AC-06 (invalid JSON)
        vm.TestMode = "invalid-json";
        await vm.RefreshShipmentsAsync();
        Assert.True(vm.HasError);
        Assert.Contains("invalid JSON", vm.ErrorMessage, StringComparison.OrdinalIgnoreCase);

        // AC-05 (unreachable port)
        vm.EndpointPath = "/api/shipments";
        vm.TestMode = string.Empty;
        vm.TimeoutSeconds = 5;
        vm.BaseUrl = "http://127.0.0.1:59999";
        await vm.RefreshShipmentsAsync();
        Assert.True(vm.HasError);
        Assert.Contains("Unable to reach", vm.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }
}
