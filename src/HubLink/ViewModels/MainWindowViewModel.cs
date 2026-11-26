using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HubLink.Models;
using HubLink.Services;

namespace HubLink.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    private readonly IShipmentApiService _apiService;
    private readonly ICsvExportService _csvExportService;
    private readonly IFileDialogService? _fileDialogService;

    private List<Shipment> _allShipments = new();
    private string _loadedBranchCode = string.Empty;

    public static IReadOnlyList<string> StatusFilterOptions { get; } =
    [
        "All",
        "Pending",
        "InTransit",
        "Delivered",
        "Exception"
    ];

    public static IReadOnlyList<string> EndpointOptions { get; } =
    [
        "/api/shipments",
        "/api/test/shipments"
    ];

    public static IReadOnlyList<string> TestModeOptions { get; } =
    [
        "",
        "normal",
        "empty",
        "bad-request",
        "server-error",
        "invalid-json",
        "delay"
    ];

    [ObservableProperty]
    private string _baseUrl;

    [ObservableProperty]
    private string _endpointPath;

    [ObservableProperty]
    private string _branchCode;

    [ObservableProperty]
    private int _timeoutSeconds;

    [ObservableProperty]
    private string _testMode;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RefreshShipmentsCommand))]
    [NotifyCanExecuteChangedFor(nameof(ExportCsvCommand))]
    private bool _isLoading;

    [ObservableProperty]
    private bool _hasError;

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private bool _isStaleData;

    [ObservableProperty]
    private bool _isEmptyResult;

    [ObservableProperty]
    private string _statusMessage = "Ready. Click Refresh to retrieve shipments.";

    [ObservableProperty]
    private string? _exportFeedbackMessage;

    [ObservableProperty]
    private bool _hasExportFeedback;

    [ObservableProperty]
    private bool _isExportError;

    [ObservableProperty]
    private string _selectedStatusFilter = "All";

    [ObservableProperty]
    private ShipmentSummary _summary = ShipmentSummary.Empty;

    public ObservableCollection<Shipment> FilteredShipments { get; } = new();

    public string VisibleCountSummary =>
        $"Showing {FilteredShipments.Count} of {_allShipments.Count} shipment(s)";

    public MainWindowViewModel(
        IShipmentApiService apiService,
        ICsvExportService csvExportService,
        HubLinkSettings settings,
        IFileDialogService? fileDialogService = null)
    {
        _apiService = apiService ?? throw new ArgumentNullException(nameof(apiService));
        _csvExportService = csvExportService ?? throw new ArgumentNullException(nameof(csvExportService));
        ArgumentNullException.ThrowIfNull(settings);
        _fileDialogService = fileDialogService;

        _baseUrl = settings.BaseUrl;
        _endpointPath = settings.EndpointPath;
        _branchCode = settings.BranchCode;
        _timeoutSeconds = settings.TimeoutSeconds;
        _testMode = settings.Mode;
    }

    partial void OnSelectedStatusFilterChanged(string value)
    {
        ApplyFilter();
    }

    private bool CanRefresh() => !IsLoading;

    private bool CanExport() => !IsLoading;

    [RelayCommand(CanExecute = nameof(CanRefresh))]
    public async Task RefreshShipmentsAsync()
    {
        if (IsLoading)
        {
            return;
        }

        IsLoading = true;
        HasError = false;
        ErrorMessage = null;
        HasExportFeedback = false;
        ExportFeedbackMessage = null;
        StatusMessage = $"Retrieving shipments for {BranchCode}...";

        try
        {
            var settings = new HubLinkSettings
            {
                BaseUrl = BaseUrl,
                EndpointPath = EndpointPath,
                BranchCode = BranchCode,
                TimeoutSeconds = TimeoutSeconds,
                Mode = TestMode
            };

            var result = await _apiService.GetShipmentsAsync(settings);

            if (result.IsSuccess)
            {
                _allShipments = result.Shipments.ToList();
                _loadedBranchCode = result.BranchCode;
                IsStaleData = false;
                HasError = false;
                ErrorMessage = null;
                IsEmptyResult = _allShipments.Count == 0;

                Summary = ShipmentSummary.FromShipments(_allShipments);
                ApplyFilter();

                var localTime = result.RetrievedAtUtc?.ToLocalTime().ToString("HH:mm:ss") ?? "";
                StatusMessage = IsEmptyResult
                    ? $"No shipments found for branch {result.BranchCode} (retrieved at {localTime})."
                    : $"Loaded {_allShipments.Count} shipment(s) for {result.BranchCode} at {localTime}.";
            }
            else
            {
                HasError = true;
                ErrorMessage = result.ErrorMessage ?? "Failed to retrieve shipments.";
                IsEmptyResult = false;

                if (_allShipments.Count > 0)
                {
                    IsStaleData = true;
                    StatusMessage = $"Refresh failed. Displaying previously loaded data for {_loadedBranchCode}.";
                }
                else
                {
                    IsStaleData = false;
                    StatusMessage = "Retrieval failed. No shipment data loaded.";
                }
            }
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand(CanExecute = nameof(CanExport))]
    public async Task ExportCsvAsync()
    {
        if (FilteredShipments.Count == 0)
        {
            HasExportFeedback = true;
            IsExportError = true;
            ExportFeedbackMessage = "No displayed shipments are available to export.";
            return;
        }

        if (_fileDialogService is null)
        {
            HasExportFeedback = true;
            IsExportError = true;
            ExportFeedbackMessage = "File dialog service is not available.";
            return;
        }

        var safeBranch = string.IsNullOrWhiteSpace(_loadedBranchCode) ? BranchCode : _loadedBranchCode;
        var filterTag = string.IsNullOrWhiteSpace(SelectedStatusFilter) ? "All" : SelectedStatusFilter;
        var suggestedName = $"hublink-{safeBranch}-{filterTag}-{DateTime.Now:yyyyMMdd-HHmmss}.csv";

        var filePath = await _fileDialogService.PickCsvSavePathAsync(suggestedName);
        if (string.IsNullOrWhiteSpace(filePath))
        {
            return;
        }

        await ExportCsvToPathAsync(filePath);
    }

    public async Task<CsvExportResult> ExportCsvToPathAsync(string filePath)
    {
        var itemsToExport = FilteredShipments.ToList();
        var result = await _csvExportService.ExportAsync(filePath, itemsToExport);

        HasExportFeedback = true;
        IsExportError = !result.IsSuccess;
        ExportFeedbackMessage = result.Message;

        return result;
    }

    private void ApplyFilter()
    {
        FilteredShipments.Clear();

        IEnumerable<Shipment> query = _allShipments;
        if (!string.IsNullOrWhiteSpace(SelectedStatusFilter) &&
            !string.Equals(SelectedStatusFilter, "All", StringComparison.OrdinalIgnoreCase))
        {
            query = query.Where(s =>
                string.Equals(s.Status, SelectedStatusFilter, StringComparison.OrdinalIgnoreCase));
        }

        foreach (var shipment in query)
        {
            FilteredShipments.Add(shipment);
        }

        OnPropertyChanged(nameof(VisibleCountSummary));
        ExportCsvCommand.NotifyCanExecuteChanged();
    }
}
