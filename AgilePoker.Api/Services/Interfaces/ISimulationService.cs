using AgilePoker.Api.Models;

namespace AgilePoker.Api.Services.Interfaces;

public interface ISimulationService
{
    Task<bool> AddBot(string roomId);
    void ResetTable(string roomId, List<Player> bots);
}