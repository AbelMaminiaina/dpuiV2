using LignageApp.Models;
using LignageApp.Services;

namespace LignageApp.Data;

public class CachedLignageRepository : ILignageRepository
{
    private readonly ILignageRepository _repository;
    private readonly ICacheService _cache;

    public CachedLignageRepository(LignageRepository repository, ICacheService cache)
    {
        _repository = repository;
        _cache = cache;
    }

    public async Task<List<NodeDto>> GetDistinctNodesAsync()
    {
        if (_cache.TryGet<List<NodeDto>>(CacheKeys.DistinctNodes, out var cached) && cached != null)
            return cached;

        var nodes = await _repository.GetDistinctNodesAsync();
        _cache.Set(CacheKeys.DistinctNodes, nodes, TimeSpan.FromHours(1));
        return nodes;
    }

    public async Task<LignageRow?> GetByPKAsync(string lanUid, string linUid, string edgDir)
    {
        var key = CacheKeys.LignageRow(lanUid, linUid, edgDir);
        if (_cache.TryGet<LignageRow>(key, out var cached) && cached != null)
            return cached;

        var row = await _repository.GetByPKAsync(lanUid, linUid, edgDir);
        if (row != null)
            _cache.Set(key, row, TimeSpan.FromMinutes(30));
        return row;
    }

    public async Task<List<LineageDto>> GetSuccessorsAsync(string lanUid, string linUid, string edgDir, bool useEdg = false, int maxRes = 500)
    {
        var key = CacheKeys.Successors(lanUid, linUid, edgDir) + (useEdg ? "_E" : "_D") + $"_{maxRes}";
        if (_cache.TryGet<List<LineageDto>>(key, out var cached) && cached != null)
            return cached;

        var successors = await _repository.GetSuccessorsAsync(lanUid, linUid, edgDir, useEdg, maxRes);
        _cache.Set(key, successors, TimeSpan.FromMinutes(30));
        return successors;
    }

    public async Task<List<LineageDto>> GetPredecessorsAsync(string lanUid, string linUid, string edgDir, bool useEdg = false, int maxRes = 500)
    {
        var key = CacheKeys.Predecessors(lanUid, linUid, edgDir) + (useEdg ? "_E" : "_D") + $"_{maxRes}";
        if (_cache.TryGet<List<LineageDto>>(key, out var cached) && cached != null)
            return cached;

        var predecessors = await _repository.GetPredecessorsAsync(lanUid, linUid, edgDir, useEdg, maxRes);
        _cache.Set(key, predecessors, TimeSpan.FromMinutes(30));
        return predecessors;
    }

    public async Task<ProgramDto?> GetProgramAsync(string lanUid)
    {
        var key = CacheKeys.Program(lanUid);
        if (_cache.TryGet<ProgramDto>(key, out var cached) && cached != null)
            return cached;

        var program = await _repository.GetProgramAsync(lanUid);
        if (program != null)
            _cache.Set(key, program, TimeSpan.FromMinutes(30));
        return program;
    }
}
