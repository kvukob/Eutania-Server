using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Server.Database;
using Server.Services.Effex;
using Server.Web;

namespace Server.Core.Sectors;

[AllowAnonymous]
[ApiController]
[Route("api/{controller}")]
[Produces("application/json")]
public class SectorController : ControllerBase
{
    private readonly SectorManager _sectorManager;
    private readonly IServiceProvider _serviceProvider;

    public SectorController(GameDbContext dbContext, IServiceProvider serviceProvider)
    {
        _sectorManager = new SectorManager(dbContext);
        _serviceProvider = serviceProvider;
    }

    [HttpGet]
    [Route("name/{name}")]
    public async Task<IActionResult> GetByName(string name)
    {
        var (sector, owner) = await _sectorManager.GetByName(name);
        var response = new ApiResponse(
            true, 
            null,
            new
            {
                Sector = new
                {
                    Name = sector.Name,
                    Planet = sector.Planet,
                    Rarity = sector.Rarity,
                    Commission = sector.Commission,
                    ForSale = sector.ForSale,
                    Price = sector.Price,
                    Owner = owner,
                    Faction = sector.Faction
                }
            }
            );
        return Ok(response);
    }

    [HttpGet]
    [Route("effects/planet/{planetName}")]
    public async Task<IActionResult> GetEffectsByPlanet(string planetName)
    {
        var effex = _serviceProvider.GetRequiredService<IEffex>();
        var effects = effex.GetEffectsByPlanet(planetName);
        var response = new ApiResponse()
        {
            Success = true,
            Message = null,
            Data = new
            {
                effects
            }
        };
        return Ok(response);
    }
}