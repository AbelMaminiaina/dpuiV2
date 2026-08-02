using LignageApp.Models;

namespace LignageApp.Data;

public interface ILignageRepository
{
    Task<List<NoeudDto>> GetNoeudsDistinctsAsync();
    Task<NoeudDto?> GetNoeudByIdAsync(int id);
    Task<List<LignageDto>> GetSuccesseursAsync(int id);
    Task<List<LignageDto>> GetPredecesseursAsync(int id);
    Task<ProgrammeDto?> GetProgrammeAsync(int id);
}
