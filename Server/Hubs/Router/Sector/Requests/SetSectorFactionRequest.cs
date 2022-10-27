namespace Server.Hubs.Router.Sector.Requests;

public class SetSectorFactionRequest
{
    public string SectorName { get; set; } = null!;
    public string FactionName { get; set; } = null!;
}