using LignageApp.Models;

namespace LignageApp.Data;

public interface ILignageRepository
{
    Task<List<NodeDto>> GetDistinctNodesAsync();
    Task<LignageRow?> GetByPKAsync(string lanUid, string linUid, string edgDir);
    Task<List<LineageDto>> GetSuccessorsAsync(string lanUid, string linUid, string edgDir);
    Task<List<LineageDto>> GetPredecessorsAsync(string lanUid, string linUid, string edgDir);
    Task<ProgramDto?> GetProgramAsync(string lanUid);
}
