using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HubLink.Models;

namespace HubLink.Services;

public interface ICsvExportService
{
    Task<CsvExportResult> ExportAsync(
        string filePath,
        IReadOnlyList<Shipment> shipments,
        CancellationToken cancellationToken = default);
}
