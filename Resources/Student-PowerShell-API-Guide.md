# HubLink - Exploring REST APIs with PowerShell

This guide is an optional learning and diagnostic resource for Performance Task 5. It is not a separate graded task and adds no submission requirements. Postman is not required.

## 1. Purpose and prerequisites

PowerShell can act as an HTTP client. `Invoke-RestMethod` sends an HTTP request and, for a normal HubLink JSON response, converts the response into PowerShell objects. Inspecting the service first helps you understand its data and behavior before writing your C# integration.

Open Windows PowerShell or PowerShell 7 from the Start menu. You need a running HubLink service and its base address from your instructor. The examples below work with the local example `http://127.0.0.1:5080` **only if the API runs on your own computer**. `127.0.0.1` means the machine making the request. For a service on the instructor's LAN computer, replace the example with the reachable address and port provided by your instructor. Do not assume the example address is available.

Set the address for the later examples, then make a simple connectivity request:

```powershell
$base = 'http://127.0.0.1:5080'  # Replace with the instructor-provided address when needed.
Invoke-RestMethod -Uri "$base/api/shipments?branchCode=BR-001" -Method Get
```

If this fails, check the address and whether the service is running; see Troubleshooting. Use the Student API Reference for the exact contract.

## 2. Make the first request

For the local example, the full request can be written as:

```powershell
$uri = "http://127.0.0.1:5080/api/shipments?branchCode=BR-001"
$response = Invoke-RestMethod -Uri $uri -Method Get
$response
```

When using another host, set `$uri = "$base/api/shipments?branchCode=BR-001"` instead. The URL identifies the resource and selected branch. `Get` asks the service to retrieve information. `$response` holds the returned PowerShell object, which contains the branch and its shipments. Later examples assume this successful BR-001 request has run.

## 3. Inspect the response object

```powershell
$response | Format-List *
$response.branchCode
$response.shipments
$response.shipments | Format-List *
$response.shipments[0] | Format-List *
$response | Get-Member
```

The dot accesses a property. `shipments` is an array nested in the response; `[0]` selects its first element, if one exists. The pipeline `|` passes the result of one command to the next. `Format-List *` displays all properties, and `Get-Member` lists available members. Do not index `[0]` when the array is empty.

## 4. Format shipment information

```powershell
$response.shipments | Format-List trackingNumber, origin, destination, status
$response.shipments | Format-Table trackingNumber, origin, destination, status -AutoSize
$response.shipments | Select-Object trackingNumber, status, destination
$response.shipments | Sort-Object destination | Select-Object trackingNumber, destination
```

Lists show fields vertically; tables place them in columns. Narrow consoles may truncate long table values. Widen the window, select fewer columns, use `Format-Table ... -Wrap`, or use a list. `Select-Object` chooses properties while `Sort-Object` orders records. Use formatting commands at the end of a display pipeline; perform calculations on the original objects.

## 5. Filter and summarize

```powershell
$inTransit = @($response.shipments | Where-Object { $_.status -eq 'InTransit' })
$inTransit | Sort-Object destination | Format-Table trackingNumber, destination, status
$response.shipments | Group-Object status | Select-Object Name, Count
$total = ($response.shipments | Measure-Object).Count
$total
$inTransit.Count
```

`$_` is the current shipment in the pipeline. The filter selects matching records, while grouping and measuring above use the **entire retrieved collection**. For BR-001, expect total 5; Pending 1, InTransit 2, Delivered 1, Exception 1. The filtered InTransit subset contains 2. This distinction matches the assignment: the status filter controls the visible list, while the summary covers all retrieved branch shipments.

## 6. Examine a JSON representation

```powershell
$response | ConvertTo-Json -Depth 5
```

`Invoke-RestMethod` interpreted the JSON into objects; `ConvertTo-Json` serializes those objects back into a JSON representation. This is useful for inspection but need not reproduce the original HTTP response byte for byte.

## 7. Test empty results

```powershell
$empty = Invoke-RestMethod -Uri "$base/api/shipments?branchCode=BR-999"
$empty.branchCode
@($empty.shipments).Count
```

The expected branch is `BR-999` and the count is 0. This is a successful HTTP 200 response with no records, distinct from an HTTP 404 response or a network failure.

## 8. Test HTTP errors

These URLs deliberately return unsuccessful HTTP statuses:

```powershell
$badRequestUri = "$base/api/test/shipments?branchCode=BR-001&mode=bad-request"
$serverErrorUri = "$base/api/test/shipments?branchCode=BR-001&mode=server-error"

foreach ($testUri in @($badRequestUri, $serverErrorUri)) {
    try {
        $result = Invoke-RestMethod -Uri $testUri -ErrorAction Stop
        "Request succeeded: $testUri"
    }
    catch {
        "Request failed: $testUri"
        $_.Exception.Message
    }
}
```

The expected responses are HTTP 400 and HTTP 500, respectively. A successful HTTP status and an unsuccessful status must be handled differently. The error text and details displayed by PowerShell may vary by version.

## 9. Test invalid JSON

```powershell
$invalidUri = "$base/api/test/shipments?branchCode=BR-001&mode=invalid-json"

try {
    $result = Invoke-RestMethod -Uri $invalidUri -ErrorAction Stop
    $result | Format-List *
}
catch {
    Write-Host "Request or parsing failed:"
    Write-Host $_.Exception.Message
}

# Make a separate request to inspect the original response body.
$raw = Invoke-WebRequest -Uri $invalidUri -UseBasicParsing
$raw.StatusCode
$raw.Content
```

This endpoint deliberately returns HTTP 200 with an incomplete JSON body. HTTP success, valid JSON, and valid application data are three separate checks: even valid JSON might not contain the expected shipment fields. Depending on PowerShell version, `Invoke-RestMethod` may raise an error or expose malformed content as a string rather than a usable object; do not assume identical parsing or error messages across versions. `Invoke-WebRequest` makes a second request to inspect the HTTP status and original response body. If the service is unreachable, that request can also fail. `-UseBasicParsing` supports Windows PowerShell 5.1 and is accepted by PowerShell 7.

## 10. Test a delayed response

```powershell
$delayUri = "$base/api/test/shipments?branchCode=BR-001&mode=delay"
try {
    Invoke-RestMethod -Uri $delayUri -TimeoutSec 3 -ErrorAction Stop
}
catch {
    "Request timed out or failed: $($_.Exception.Message)"
}
```

The endpoint waits 10 seconds, so a 3-second client timeout should interrupt the request before its normal response. A timeout means the client stopped waiting; it is not an empty result. A desktop app should stay responsive and give feedback while waiting. Retrying should be deliberate and bounded rather than an unrestricted loop.

## 11. Troubleshooting

| Symptom | Check or action |
|---|---|
| Connection fails | Confirm that the API is running and the host and port are correct. |
| Localhost fails with an instructor-hosted service | Use the instructor-provided LAN address; localhost means your own machine. |
| LAN address is unreachable | Ask the instructor to check network connectivity, server binding, and firewall access. |
| HTTP 400 | Use one case-sensitive branch value such as `BR-001`; remove repeated `branchCode` parameters. |
| Timeout | Check connectivity and timeout settings; a short timeout is expected to fail for `mode=delay`. |
| Invalid JSON | Inspect raw content and verify the endpoint and mode; invalid JSON is intentional in its test mode. |
| Empty shipments | Check the returned branch; a valid branch with no records is a successful response. |
| Output or error text differs | Check `$PSVersionTable.PSVersion`; Windows PowerShell 5.1 and PowerShell 7 may display or parse responses differently. |

## 12. Connection to the Avalonia assignment

PowerShell lets you explore requests and returned objects before building the application. In your desktop client, `HttpClient` makes the actual request; `async`/`await` supports responsive retrieval; JSON deserialization maps the response into application models; ViewModels and data binding present and filter the data; and CSV export writes the currently displayed records.

Manual PowerShell exploration is a diagnostic aid, not a substitute for the required independent C# and Avalonia implementation. Refer to the handout for the assessment requirements and the Student API Reference for endpoint details.
