using LignageApp.Models;

namespace LignageApp.Data;

public interface ILignageRepository
{
    Task<List<NodeDto>> GetDistinctNodesAsync();
    Task<LignageRow?> GetByPKAsync(string lanUid, string linUid, string edgDir);
    Task<List<LineageDto>> GetSuccessorsAsync(string lanUid, string linUid, string edgDir, bool useEdg = false, int maxRes = 500);
    Task<List<LineageDto>> GetPredecessorsAsync(string lanUid, string linUid, string edgDir, bool useEdg = false, int maxRes = 500);
    Task<ProgramDto?> GetProgramAsync(string lanUid);
}
