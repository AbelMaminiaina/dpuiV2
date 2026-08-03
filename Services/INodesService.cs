using LignageApp.Models;

namespace LignageApp.Services;

public interface INodesService
{
    Task<List<NodeDto>> GetNodesAsync();
}
