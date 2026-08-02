using LignageApp.Models;

namespace LignageApp.Services;

public interface INoeudsService
{
    Task<List<NoeudDto>> GetNoeudsAsync();
}
