# HubLink — Regional Logistics Data Exchange

Desktop client built with **C#**, **.NET 10**, and **Avalonia UI (MVVM)** for retrieving, filtering, summarizing, and exporting branch shipment records from the HubLink REST API.

## Project Structure

- `src/HubLink/` — Avalonia MVVM desktop application
  - `Models/` — `HubLinkSettings`, `Shipment`, `ShipmentSummary`, `ShipmentRetrievalResult`, `CsvExportResult`
  - `Services/` — `ShipmentApiService` (`HttpClient` + `System.Text.Json`), `CsvExportService` (RFC 4180 escaping), `JsonSettingsService`, `AvaloniaFileDialogService`
  - `ViewModels/` — `MainWindowViewModel` (CommunityToolkit.Mvvm)
  - `Views/` — `MainWindow.axaml`
- `tests/HubLink.Tests/` — xUnit unit and live-API integration tests
- `hublink-BR-001-sample.csv` — Sample CSV export generated from live `BR-001` API data

## Configuration

Default connection settings are stored in `src/HubLink/appsettings.json` (copied to the build output directory):

```json
{
  "HubLinkApi": {
    "BaseUrl": "http://127.0.0.1:5080",
    "EndpointPath": "/api/shipments",
    "BranchCode": "BR-001",
    "TimeoutSeconds": 5,
    "Mode": ""
  }
}
```

- **BaseUrl**: Service base address (for example, `http://127.0.0.1:5080` when running the local test server, or the instructor-provided LAN URL).
- **EndpointPath**: `/api/shipments` for normal operation, or `/api/test/shipments` for controlled test scenarios.
- **BranchCode**: Case-sensitive branch identifier (e.g., `BR-001`, `BR-002`, `BR-003`, `BR-999`).
- **TimeoutSeconds**: HTTP request timeout in seconds (default `5`, shorter than the 10-second `mode=delay` test).
- **Mode**: Optional query parameter for `/api/test/shipments` (`normal`, `empty`, `bad-request`, `server-error`, `invalid-json`, `delay`).

All of these values can also be adjusted at runtime in the application's top connection bar without editing code.

## Prerequisites & Running

1. Start the HubLink API service (for local testing, extract `Resources/HubLink-PT5-Local-Server-Windows-x64.zip` and run `Start-HubLink.cmd`).
2. Restore dependencies:
   ```powershell
   dotnet restore HubLink.sln
   ```
3. Run the desktop application:
   ```powershell
   dotnet run --project src/HubLink
   ```
4. Run the unit tests:
   ```powershell
   dotnet test HubLink.sln
   ```
   To also run the live API integration test against `http://127.0.0.1:5080`, set `HUBLINK_LIVE_TEST=1` before running `dotnet test`.

## Acknowledgments

AI-assisted development tools were used to help scaffold the Avalonia MVVM project structure, service implementations, unit tests, and documentation. All code and outputs were reviewed and verified against the local HubLink API.
