# HubLink Verification Checklist

This checklist records verification of Acceptance Criteria **AC-01** through **AC-08** from *Performance Task 5: HubLink*.

## Automated & Live-API Test Evidence

- **Unit Tests (`tests/HubLink.Tests/`)**: `dotnet test HubLink.sln --no-restore` — **13/13 passed** (`ShipmentApiServiceTests`, `CsvExportServiceTests`, `MainWindowViewModelTests`).
- **Live Local API Integration Test (`LiveApiIntegrationTests`)**: Executed with `HubLink.Api.exe` running on `http://127.0.0.1:5080` (`HUBLINK_LIVE_TEST=1 dotnet test HubLink.sln --no-restore`) — **14/14 passed**.

## Acceptance Scenario Status

| ID | Scenario | Status | How Checked |
|---|---|---|---|
| AC-01 | Retrieve shipment information from the provided API for a configured branch. | Verified | Live HTTP `GET /api/shipments?branchCode=BR-001` via `ShipmentApiService` and `MainWindowViewModel` returned 5 shipments; unit-tested JSON parsing in `ShipmentApiServiceTests`. |
| AC-02 | Apply different shipment status filters. | Verified | `MainWindowViewModel` filtered `BR-001` to `InTransit` (2 records) in memory without extra HTTP calls and restored all 5 records when `All` was selected. |
| AC-03 | Examine the operational summary. | Verified | `ShipmentSummary` for live `BR-001` verified as Total `5`, Pending `1`, InTransit `2`, Delivered `1`, Exception `1`, remaining unchanged while status filter is active. |
| AC-04 | Refresh information from the service. | Verified | Async `RefreshShipmentsCommand` uses `async`/`await`, sets `IsLoading` with `CanExecute` guard to prevent overlapping calls, and updates state upon completion. |
| AC-05 | Encounter an unavailable service, request timeout, or unsuccessful HTTP response. | Verified | Verified against live `/api/test/shipments` with `mode=bad-request` (HTTP 400), `mode=server-error` (HTTP 500), `mode=delay` (2s timeout), and unreachable port (`127.0.0.1:59999`), plus stale-data preservation and `Retry` button in `MainWindowViewModelTests`. |
| AC-06 | Receive an empty or invalid response. | Verified | Live `BR-999` returned HTTP 200 with 0 shipments (`IsEmptyResult = true`, `HasError = false`); live `mode=invalid-json` and unit-tested missing-fields JSON produced `HasError = true` with clear error feedback. |
| AC-07 | Export shipment information while a status filter is active. | Verified | Unit test `RefreshFilterAndSummary_BehaveCorrectlyAndExportFilteredOnly` exported only the 2 `InTransit` rows; `CsvExportServiceTests` verified RFC 4180 escaping of commas and quotes (`Butuan, Agusan del Norte` and `Depot "North"`). Live `BR-001` export saved to `hublink-BR-001-sample.csv`. |
| AC-08 | Attempt an export that cannot be completed. | Verified | `CsvExportServiceTests.ExportAsync_EmptyShipments_ReturnsFailureWithoutCreatingFile` and `ExportAsync_UnwritablePath_ReturnsFailure` confirmed clear failure feedback without false success. |
