using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Server.Database;
using Server.Web;

namespace Server.Core.Market;

[Authorize]
[ApiController]
[Route("api/{controller}")]
[Produces("application/json")]
public class MarketController : ControllerBase
{
    private readonly MarketManager _marketManager;

    public MarketController(GameDbContext dbContext, IServiceProvider serviceProvider)
    {
        _marketManager = new MarketManager(dbContext, serviceProvider);
    }
    
    [HttpGet, Route("listings/{id}")]
    public async Task<IActionResult> GetListingById(int id)
    {
        var listing = await _marketManager.GetById(id);
        var response = new ApiResponse();
        if (listing is null)
        {
            response.Success = false;
            response.Message = "Listing not found";
        }
        else
        {
            response.Success = true;
            response.Data = new
            {
                Listing = new
                {
                    Item = new
                    {
                        Name = listing.Item.GameItem.Name,
                        Type = listing.Item.GameItem.Type,
                        MintNumber = listing.Item.MintNumber,
                        Foil = listing.Item.Foil
                    },
                    Id = listing.Id,
                    Quantity = listing.Quantity,
                    Price = listing.Price,
                    Seller = listing.Seller
                }
            };
        }
        return Ok(response);
    }

    [HttpGet, Route("listings/self")]
    public async Task<IActionResult> GetSelfListings()
    {
        var playerId = this.GetPlayerId(HttpContext);
        var listings = await _marketManager.GetBySelfListed(playerId);
        var response = new ApiResponse();
        response.Success = true;
        response.Data = listings;
        return Ok(response);
    }


    [HttpGet, Route("listings/latest")]
    public async Task<IActionResult> GetLatest()
    {
        var (listings, count) = await _marketManager.GetLatest();
        var response = new ApiResponse(true, null, new
        {
            Listings = listings,
            count = count
        });
        return Ok(response);
    }

    [HttpGet, Route("listings/type/{itemType}/{page}")]
    public async Task<IActionResult> GetListingsByType(string itemType, int page)
    {
        var (listings, count) = await _marketManager.GetListingsByType(page, itemType);
        var response = new ApiResponse(true, null, new
        {
            Listings = listings,
            Count = count
        });
        return Ok(response);
    }

    [HttpGet, Route("listings/name/{itemName}/{page}")]
    public async Task<IActionResult> GetListingsByItemName(string itemName, int page)
    {
        var (listings, count) = await _marketManager.GetListingsByItemName(page, itemName);
        var response = new ApiResponse(true, null, new
        {
            Listings = listings,
            Count = count
        });
        return Ok(response);
    }

    [HttpGet, Route("listings/player/sectors")]
    public async Task<IActionResult> GetPlayerSectors()
    {
        var playerId = this.GetPlayerId(HttpContext);
        var listings = await _marketManager.GetPlayerSectors(playerId);
        var response = new ApiResponse();
        response.Success = true;
        response.Data = listings;
        return Ok(response);
    }

    [HttpGet, Route("listings/sectors/{page}")]
    public async Task<IActionResult> GetSectors(int page, [FromQuery] string? planet = null,
        [FromQuery] string? rarity = null)
    {
        var (sectors, count) = await _marketManager.GetSectors(page, planet, rarity);
        var response = new ApiResponse(true, null, new
        {
            sectors,
            Count = count
        });

        return Ok(response);
    }
}