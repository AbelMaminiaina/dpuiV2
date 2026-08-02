using LignageApp.Data;
using LignageApp.Models;

namespace LignageApp.Services;

public class NoeudsService : INoeudsService
{
    private readonly ILignageRepository _repository;

    public NoeudsService(ILignageRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<NoeudDto>> GetNoeudsAsync()
    {
        return await _repository.GetNoeudsDistinctsAsync();
    }
}
