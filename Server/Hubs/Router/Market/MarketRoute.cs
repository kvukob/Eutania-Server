using System.Text.Json;
using Castle.Core.Internal;
using Server.Core.Market;
using Server.Database;
using Server.Hubs.Router.Market.Requests;
using Server.Web;

namespace Server.Hubs.Router.Market;

public class MarketRoute
{
    private readonly MarketManager _marketManager;

    public MarketRoute(GameDbContext db, IServiceProvider serviceProvider)
    {
        _marketManager = new MarketManager(db, serviceProvider);
    }

    public async Task<HubResponse> Handle(string command, string request, Guid playerId)
    {
        return command switch
        {
            "buySector" => await BuySector(request, playerId),
            "sellSector" => await SellSector(request, playerId),
            "cancelSector" => await CancelSector(request, playerId),
            "buyListing" => await BuyListing(request, playerId),
            "sellListing" => await SellListing(request, playerId),
            "cancelListing" => await CancelListing(request, playerId),
            _ => new HubResponse()
        };
    }


    private async Task<HubResponse> SellSector(string request, Guid playerId)
    {
        var req = JsonSerializer.Deserialize<SellSectorRequest>(request);
        if (req is null) return new HubResponse() {Success = false};

        var listed = await _marketManager.SellSector(playerId, req.SectorName, req.Price);
        var response = new HubResponse()
        {
            Success = listed,
            Message = listed ? $"Listed {req.SectorName}." : $"There was a problem listing {req.SectorName}"
        };

        return response;
    }

    private async Task<HubResponse> BuySector(string request, Guid playerId)
    {
        var req = JsonSerializer.Deserialize<BuySectorRequest>(request);
        if (req is null) return new HubResponse() {Success = false};

        var bought = await _marketManager.BuySector(playerId, req.SectorName);
        var response = new HubResponse()
        {
            Success = bought,
            Message = bought ? $"Bought {req.SectorName}" : $"There was a problem buying {req.SectorName}"
        };
        return response;
    }
    
    private async Task<HubResponse> CancelSector(string request, Guid playerId)
    {
        var req = JsonSerializer.Deserialize<CancelSectorRequest>(request);
        if (req is null) return new HubResponse() {Success = false};
        
        var success = await _marketManager.CancelSector(playerId, req.SectorName);
        var response = new HubResponse()
        {
            Success = success,
        };
        return response;
    }

    private async Task<HubResponse> BuyListing(string request, Guid playerId)
    {
        var req = JsonSerializer.Deserialize<BuyListingRequest>(request);
        if (req is null) return new HubResponse() {Success = false};
        
        var message = await _marketManager.BuyListing(playerId, req.ListingId);
        var response = new HubResponse()
        {
            Success = !message.IsNullOrEmpty()
        };
        if (!message.IsNullOrEmpty())
            response.Message = message;
        return response;
    }
    
    private async Task<HubResponse> SellListing(string request, Guid playerId)
    {
        var req = JsonSerializer.Deserialize<SellListingRequest>(request);
        if (req is null) return new HubResponse() {Success = false};
        
        var message = await _marketManager.SellListing(playerId, req.Identifier, req.Quantity, req.Price);
        var response = new HubResponse()
        {
            Success = !message.IsNullOrEmpty()
        };
        if (!message.IsNullOrEmpty())
            response.Message = message;
        return response;
    }
    
    private async Task<HubResponse> CancelListing(string request, Guid playerId)
    {
        var req = JsonSerializer.Deserialize<CancelListingRequest>(request);
        if (req is null) return new HubResponse() {Success = false};
        
        var (success, message) = await _marketManager.CancelListing(playerId, req.ListingId);
        var response = new HubResponse()
        {
            Success = success,
            Message = message
        };
        return response;
    }
}