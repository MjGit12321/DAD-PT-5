using System.Threading;
using System.Threading.Tasks;
using HubLink.Models;

namespace HubLink.Services;

public interface IShipmentApiService
{
    Task<ShipmentRetrievalResult> GetShipmentsAsync(
        HubLinkSettings settings,
        CancellationToken cancellationToken = default);
}
