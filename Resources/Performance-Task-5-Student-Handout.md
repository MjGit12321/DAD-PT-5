# ITSD81 — Desktop Application Development
## Performance Task 5: HubLink — Regional Logistics Data Exchange

**Course Component:** Lecture  
**Unit:** V — External Services, Async Processing & Reporting  
**Duration:** Two Weeks  
**Collaboration:** Pair Activity  
**Output:** Functional API-Integrated Desktop Application and Essential Supporting Documentation

---

### I. Performance Task Overview

A regional logistics company operates a centralized shipment information service used by several partner branches.

The central provider maintains shipment records, including tracking numbers, destinations, shipment statuses, and recent status updates.

Smaller logistics branches rely on this information to monitor shipments, assist customers, and prepare operational reports.

However, partner branches do not directly manage or access the central provider's database. Instead, the central company makes selected shipment information available through a REST API.

Your pair will act as a development team responsible for creating **HubLink**, a desktop application that connects to the centralized shipment service.

The application must retrieve shipment information, interpret the received data, present it in a useful format, and export selected records into a CSV report.

It must also handle situations in which the external service is unavailable, responds with an error, or returns unusable information.

**The objective is to demonstrate how a desktop application integrates an external service, processes structured data, performs asynchronous operations, and produces useful outputs.**

HubLink is a client application. Students are not required to develop the central API server or implement a local relational database.

### II. Intended Learning Outcomes

Upon completing this Performance Task, students should be able to:

1. Explain the role of REST APIs in communication between separate systems.
2. Consume an external HTTP service using C# and .NET.
3. Interpret JSON responses and transform them into application data.
4. Retrieve and display information asynchronously without unnecessarily blocking the user interface.
5. Handle service failures, invalid responses, and unsuccessful requests.
6. Process retrieved information through meaningful filtering and summarization.
7. Export application data into a properly structured CSV file.
8. Maintain appropriate separation between external communication, application logic, and presentation.

### III. Business Scenario

The central logistics provider manages shipments belonging to several partner branches.

Each shipment contains information identifying the shipment, its branch, origin, destination, current status, and most recent update.

A partner branch requires a desktop application that allows its operations staff to:

- Retrieve shipment information from the central provider.
- View shipments associated with their branch.
- Filter shipments according to their current status.
- Examine a simple summary of retrieved shipments.
- Export relevant shipment information for operational use.

The central provider remains responsible for maintaining shipment records.

Partner branches are consumers of the information and cannot modify shipment records through HubLink.

The application shall obtain its shipment information through an instructor-provided HTTP API.

The instructor will provide the API base address and branch identifiers. Consult the separate **HubLink Student API Reference** for the data contract and controlled test scenarios. The optional **PowerShell API Exploration Guide** explains how to inspect the API manually; it is not a separate graded task.

### IV. Required Application Features

**A. External Shipment Retrieval**

The application must retrieve shipment information from the provided REST API through an actual HTTP request.

The request must use the configured service address and branch identifier.

The application must interpret the returned JSON and transform the relevant information into suitable application data.

Shipment information must not be represented solely through hard-coded sample records.

**B. Shipment Display and Status Filtering**

The application must display the retrieved shipments in an understandable interface.

At minimum, each shipment must show:

- Tracking number
- Origin
- Destination
- Current status
- Last updated date and time

The user must be able to filter retrieved shipments by status, including an option to display all statuses.

Filtering may operate on the successfully retrieved information without sending an additional HTTP request.

**C. Operational Summary**

The application must provide a simple summary of the currently retrieved branch shipments.

The summary must identify:

- Total number of retrieved shipments.
- Number of Pending shipments.
- Number of InTransit shipments.
- Number of Delivered shipments.
- Number of Exception shipments.

The summary must reflect the successfully retrieved data rather than fixed or manually entered totals.

The summary represents all retrieved shipments for the configured branch, while the status filter controls the visible shipment list.

**D. Asynchronous Retrieval and Refresh**

External requests must be performed asynchronously.

The application must remain responsive while a request is in progress.

Users must receive an appropriate indication that information is being retrieved.

The application must provide a means of refreshing shipment information from the external service.

It must prevent confusing overlapping operations or misleading loading states.

**E. Error Handling and Recovery**

The application must handle unsuccessful external operations appropriately.

Relevant conditions include:

- The external service cannot be reached.
- The request times out.
- The service returns an unsuccessful HTTP status.
- The response cannot be interpreted as the expected shipment information.

When retrieval fails, the application must provide an understandable error message.

The application must not present a failed request as a successful refresh.

Previously retrieved information may remain visible after a failed refresh only if it is clearly identified as previously loaded information rather than newly retrieved data.

The user must have a reasonable way to retry a failed request.

An empty successful response must be distinguished from a connection or processing failure.

**F. CSV Shipment Export**

The application must allow staff to export the currently displayed shipment records into a CSV file.

The export must reflect the active status filter.

The CSV file must contain the following columns, in order:

1. TrackingNumber
2. Origin
3. Destination
4. Status
5. LastUpdatedUtc

The file must include a header row and one data row for each exported shipment.

CSV values must be correctly formatted, including appropriate handling of commas, quotation marks, and other characters requiring CSV escaping.

The exported values must correspond to the shipment information retrieved from the API.

The application must provide feedback when the export succeeds or fails.

If no shipment records are available for export, the application should communicate that condition rather than producing a misleading report.

### V. Mandatory Operational Rules

| ID | Operational Rule |
|---|---|
| OR-01 | Shipment information must originate from an actual HTTP request to the provided test service. |
| OR-02 | The application must retrieve information for the configured branch identifier. |
| OR-03 | JSON responses must be interpreted into appropriate application data before presentation. |
| OR-04 | Status filters and shipment summaries must reflect the retrieved information accurately. |
| OR-05 | An unsuccessful HTTP request must not be treated as a successful retrieval. |
| OR-06 | External requests must not unnecessarily block the desktop user interface. |
| OR-07 | A successful response containing no shipments must be treated as an empty result rather than a failure. |
| OR-08 | CSV exports must contain the shipment records currently displayed under the active filter. |
| OR-09 | The exported file must contain properly structured CSV information. |
| OR-10 | Errors during external retrieval or file export must produce understandable feedback. |

The application is intended for information retrieval and reporting.

Creating, modifying, deleting, or synchronizing shipment records with the central provider is outside the required scope.

### VI. Technical Requirements

The application must use:

- C# and .NET.
- Avalonia UI and XAML.
- Model–View–ViewModel (MVVM).
- `HttpClient` or an appropriate .NET HTTP client abstraction.
- JSON serialization/deserialization facilities.
- Asynchronous programming using `async` and `await`.
- Appropriate separation of concerns.
- Standard .NET file operations or a suitable CSV library for exporting data.

The external service address and branch identifier must be configurable without editing the application's business logic.

Students may use application settings, configuration files, environment variables, or another reasonable approach.

The controlled test scenarios use a different endpoint path and a `mode` query parameter. For verification, you may temporarily configure the complete request URL or configure the endpoint path and test parameters. Restore the normal endpoint when testing is complete.

HTTP communication and JSON interpretation must be separated appropriately from UI presentation.

Views must not contain the complete API communication or data-processing implementation.

Students may determine their own service abstractions, models, ViewModels, interface layout, and CSV-generation approach.

No particular class names, method signatures, folder arrangements, or complete implementation algorithms are prescribed.

Students may reuse generic Avalonia project scaffolding or suitable infrastructure from previous coursework. HubLink-specific integration, processing, and export behavior must still be implemented for this task.

### VII. Scope and Limitations

The following functionality is not required:

- Development or hosting of the central REST API.
- A local relational database.
- Entity Framework Core migrations.
- Shipment creation, editing, or deletion.
- Synchronization of local and remote databases.
- Authentication or staff-account management.
- Paid API subscriptions or commercial cloud infrastructure.
- Integration with multiple external providers.
- CSV or JSON file imports.
- PDF or Excel report generation.
- Advanced dashboards and visual analytics.
- Background synchronization or scheduled retrieval.
- Sophisticated retry frameworks or distributed messaging.
- Self-contained deployment packages.

Cancellation controls, progress bars, additional export formats, and advanced logging may be explored as extensions but are not mandatory.

Optional functionality will not replace incomplete mandatory requirements or provide additional credit beyond the published assessment rubric.

### VIII. Acceptance and Verification Scenarios

The completed application must satisfy the following scenarios.

| ID | Scenario | Expected Result |
|---|---|---|
| AC-01 | Retrieve shipment information from the provided API for a configured branch. | The application makes a real HTTP request and correctly displays the returned shipment information. |
| AC-02 | Apply different shipment status filters. | Only records matching the selected status appear, and the All option restores the full retrieved list. |
| AC-03 | Examine the operational summary. | Total and status counts agree with the retrieved shipment records. |
| AC-04 | Refresh information from the service. | The UI remains responsive, indicates loading appropriately, and displays the updated response upon success. |
| AC-05 | Encounter an unavailable service, request timeout, or unsuccessful HTTP response. | The application gives understandable feedback, does not falsely report success, and supports retrying. |
| AC-06 | Receive an empty or invalid response. | A valid empty response is handled as no data; an unusable response produces appropriate error feedback. |
| AC-07 | Export shipment information while a status filter is active. | The resulting CSV contains the correct header and exactly the displayed records, with accurate values and valid formatting. |
| AC-08 | Attempt an export that cannot be completed. | The application handles the failure and provides understandable feedback without claiming the file was successfully created. |

The instructor will provide or identify suitable API test conditions for scenarios involving service failures or invalid responses.

Students are not required to build a separate API server or artificial fault-injection framework to satisfy these tests.

Verification must consider actual HTTP behavior, processed data, user-visible results, and exported file contents.

A successful application build or screenshots alone do not establish correct integration behavior.

### IX. Required Deliverables

Each pair must provide:

1. **Application Source** — The complete solution or repository containing the functional API-integrated desktop application and the files required to build and run it.

2. **README** — Concise instructions for configuring the provided API address and branch identifier, restoring dependencies, and running the application.

3. **Verification Checklist** — A brief record identifying which acceptance scenarios have been verified, how they were checked, and which remain incomplete.

4. **CSV Export Sample** — One sample file produced by the application using shipment records retrieved from the provided test API.

Documentation should be concise and relevant to the actual implementation.

An extensive technical report, formal presentation, or separate architecture defense document is not required.

Do not include credentials, secret values, compiled temporary folders, or unrelated generated files.

### X. Assessment Rubric

| Criterion | Weight |
|---|---:|
| REST API integration and correct HTTP communication | 30% |
| JSON interpretation, shipment processing, filtering, and summary accuracy | 20% |
| Asynchronous operations, error handling, and recovery | 20% |
| CSV export correctness and operational usefulness | 20% |
| Essential interface behavior, configuration, and verification evidence | 10% |
| **TOTAL** | **100%** |

Proportional credit may be awarded for genuine, demonstrable work satisfying part of the published requirements.

Correct HTTP integration must be verified through actual service communication or appropriate technical evidence.

JSON interpretation, filtering, summarization, and CSV export shall be assessed according to their demonstrated correctness.

The same underlying defect must not be repeatedly penalized under unrelated criteria.

Optional functionality is not required for full credit.

**Late Submission Policy:** A valid late submission incurs a deduction of **20 points** from the earned technical score, with a minimum resulting score of zero.

### XI. Collaboration and Academic Integrity

Students shall work in pairs. Both members must be identified, and one submission per pair is sufficient.

Both members are responsible for understanding the submitted application.

Students may consult official documentation, development examples, third-party libraries, and AI-assisted development tools, subject to institutional policies.

Substantive external or AI-assisted contributions must be acknowledged briefly in the project documentation.

Students remain responsible for the correctness of their submission and their ability to explain the implemented features.

Code similarity alone does not establish academic misconduct.

---

**End of Performance Task 5**
