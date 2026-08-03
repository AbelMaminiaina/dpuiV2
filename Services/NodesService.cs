using LignageApp.Data;
using LignageApp.Models;

namespace LignageApp.Services;

public class NodesService : INodesService
{
    private readonly ILignageRepository _repository;

    public NodesService(ILignageRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<NodeDto>> GetNodesAsync()
    {
        return await _repository.GetDistinctNodesAsync();
    }
}
