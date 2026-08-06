using LignageApp.Models;

namespace LignageApp.Services;

public interface IConsultationService
{
    string CurrentId { get; }
    string CurrentNode { get; }
    List<LineageDto> Successors { get; }
    List<LineageDto> Predecessors { get; }
    ProgramDto? Program { get; }
    bool CanGoBack { get; }
    bool CanGoForward { get; }
    List<HistoryEntry> History { get; }
    int CurrentIndex { get; }

    Task InitializeAsync(string lanUid, string linUid, string edgDir, bool useEdg = false);
    Task NavigateToAsync(string lanUid, string linUid, string edgDir, bool useEdg = false);
    void GoBack();
    void GoBackWithRemove();
    void GoForward();
    void GoTo(int index);
}
