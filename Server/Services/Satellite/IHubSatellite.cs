namespace Server.Services.Satellite;

public interface IHubSatellite
{
    Task SendToClient(string playerId, string command, string message);
}