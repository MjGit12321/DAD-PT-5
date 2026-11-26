# HubLink - Student API Reference

The instructor provides the service base URL. `http://127.0.0.1:5080` is an example only when the service runs on your own machine. `127.0.0.1` always refers to the machine making the request. For an instructor-hosted LAN service, use the instructor-provided reachable host and port instead. Append the endpoint path and query string to that base URL. Both endpoints are read-only GET requests. All data is fictional; no login is required.

## Branch identifiers

Supply exactly one `branchCode`, case-sensitive, matching `BR-` and three ASCII digits (for example `BR-001`). Missing, empty, malformed, lowercase, or repeated values return HTTP 400 and JSON such as:

```json
{"error":"Provide exactly one branchCode in the format BR-001."}
```

Available fixture branches are `BR-001`, `BR-002`, and `BR-003`. A syntactically valid branch without records, such as `BR-999`, returns HTTP 200 with an empty `shipments` array. This is a successful result.

## Normal shipment retrieval

`GET /api/shipments?branchCode=BR-001`

HTTP 200, `application/json`:

```json
{
  "branchCode": "BR-001",
  "shipments": [
    {
      "trackingNumber": "HUB-1001",
      "branchCode": "BR-001",
      "origin": "Cagayan de Oro",
      "destination": "Valencia",
      "status": "InTransit",
      "lastUpdatedUtc": "2026-11-10T08:30:00Z"
    }
  ]
}
```

The example shows one item; the actual BR-001 response includes five. All results belong to the requested branch. The outer `branchCode` identifies the requested branch; each shipment has a unique `trackingNumber`, its `branchCode`, `origin`, `destination`, `status`, and `lastUpdatedUtc` as an ISO 8601 UTC timestamp. Status values are exactly `Pending`, `InTransit`, `Delivered`, and `Exception`. Some fictional location values contain commas or quotation marks so CSV exports can be checked.

Fixture counts are BR-001: 5, BR-002: 2, BR-003: 1, and BR-999: 0. For BR-001 the status counts are Pending 1, InTransit 2, Delivered 1, and Exception 1.

## Controlled test endpoint

`GET /api/test/shipments?branchCode=BR-001&mode={mode}`

`branchCode` follows the same rules as the normal endpoint. The optional `mode` defaults to `normal`; it is case-sensitive and must not be repeated. An unknown or repeated mode returns HTTP 400. Modes never change the normal endpoint or stored fixtures.

| mode | Result |
|---|---|
| `normal` | HTTP 200 with the same data and JSON shape as normal retrieval. |
| `empty` | HTTP 200 with the requested `branchCode` and an empty `shipments` array. |
| `bad-request` | HTTP 400 with `{"error":"Simulated bad request."}`. |
| `server-error` | HTTP 500 with `{"error":"Simulated server error."}`. |
| `invalid-json` | HTTP 200 with `application/json` but an intentionally incomplete JSON body. |
| `delay` | Waits 10 seconds, then returns the normal HTTP 200 response unless the request is cancelled. Configure a client timeout shorter than 10 seconds to test timeout handling. |

For example, use `GET /api/test/shipments?branchCode=BR-001&mode=server-error` to test an HTTP failure. A stopped server or an instructor-provided unreachable address can be used to test connection failure. The normal endpoint does not interpret the `mode` parameter.

The test path differs from the normal path. Configure the complete request URL or both the endpoint path and test query parameters when exercising these modes; changing only the service host will not select a mode. Return to the normal path afterward. The PowerShell API Exploration Guide provides manual request examples.
