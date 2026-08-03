using LignageApp.Models;

namespace LignageApp.Data;

public interface ILignageRepository
{
    Task<List<NoeudDto>> GetNoeudsDistinctsAsync();
    Task<LignageRow?> GetByPKAsync(string lnaUid, string linUid, string edgDir);
    Task<List<LignageDto>> GetSuccesseursAsync(string lnaUid, string linUid, string edgDir);
    Task<List<LignageDto>> GetPredecesseursAsync(string lnaUid, string linUid, string edgDir);
    Task<ProgrammeDto?> GetProgrammeAsync(string lnaUid);
}
