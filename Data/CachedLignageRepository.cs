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

    public async Task<List<NoeudDto>> GetNoeudsDistinctsAsync()
    {
        if (_cache.TryGet<List<NoeudDto>>(CacheKeys.NoeudsDistincts, out var cached) && cached != null)
            return cached;

        var noeuds = await _repository.GetNoeudsDistinctsAsync();
        _cache.Set(CacheKeys.NoeudsDistincts, noeuds, TimeSpan.FromHours(1));
        return noeuds;
    }

    public async Task<LignageRow?> GetByPKAsync(string lnaUid, string linUid, string edgDir)
    {
        var key = CacheKeys.LignageRow(lnaUid, linUid, edgDir);
        if (_cache.TryGet<LignageRow>(key, out var cached) && cached != null)
            return cached;

        var row = await _repository.GetByPKAsync(lnaUid, linUid, edgDir);
        if (row != null)
            _cache.Set(key, row, TimeSpan.FromMinutes(30));
        return row;
    }

    public async Task<List<LignageDto>> GetSuccesseursAsync(string lnaUid, string linUid, string edgDir)
    {
        var key = CacheKeys.Successeurs(lnaUid, linUid, edgDir);
        if (_cache.TryGet<List<LignageDto>>(key, out var cached) && cached != null)
            return cached;

        var successeurs = await _repository.GetSuccesseursAsync(lnaUid, linUid, edgDir);
        _cache.Set(key, successeurs, TimeSpan.FromMinutes(30));
        return successeurs;
    }

    public async Task<List<LignageDto>> GetPredecesseursAsync(string lnaUid, string linUid, string edgDir)
    {
        var key = CacheKeys.Predecesseurs(lnaUid, linUid, edgDir);
        if (_cache.TryGet<List<LignageDto>>(key, out var cached) && cached != null)
            return cached;

        var predecesseurs = await _repository.GetPredecesseursAsync(lnaUid, linUid, edgDir);
        _cache.Set(key, predecesseurs, TimeSpan.FromMinutes(30));
        return predecesseurs;
    }

    public async Task<ProgrammeDto?> GetProgrammeAsync(string lnaUid)
    {
        var key = CacheKeys.Programme(lnaUid);
        if (_cache.TryGet<ProgrammeDto>(key, out var cached) && cached != null)
            return cached;

        var programme = await _repository.GetProgrammeAsync(lnaUid);
        if (programme != null)
            _cache.Set(key, programme, TimeSpan.FromMinutes(30));
        return programme;
    }
}
