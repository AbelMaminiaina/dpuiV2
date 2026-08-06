using LignageApp.Data;
using LignageApp.Models;

namespace LignageApp.Services;

public class ConsultationService : IConsultationService
{
    private readonly ILignageRepository _repository;
    private readonly NavigationSession _session;

    public string CurrentId => _session.CurrentId;
    public string CurrentNode => _session.CurrentNode;
    public List<LineageDto> Successors { get; private set; } = new();
    public List<LineageDto> Predecessors { get; private set; } = new();
    public ProgramDto? Program { get; private set; }
    public bool CanGoBack => _session.CanGoBack;
    public bool CanGoForward => _session.CanGoForward;
    public List<HistoryEntry> History => _session.GetHistory();
    public int CurrentIndex => _session.CurrentIndex;

    public ConsultationService(ILignageRepository repository, NavigationSession session)
    {
        _repository = repository;
        _session = session;
    }

    public async Task InitializeAsync(string lanUid, string linUid, string edgDir, bool useEdg = false)
    {
        _session.Reset();
        await LoadFromDatabaseAsync(lanUid, linUid, edgDir, useEdg);

        _session.Add(new HistoryEntry
        {
            Id = _session.CurrentId,
            Node = _session.CurrentNode,
            Successors = Successors,
            Predecessors = Predecessors,
            Program = Program
        });
    }

    public async Task NavigateToAsync(string lanUid, string linUid, string edgDir, bool useEdg = false)
    {
        await LoadFromDatabaseAsync(lanUid, linUid, edgDir, useEdg);

        _session.Add(new HistoryEntry
        {
            Id = _session.CurrentId,
            Node = _session.CurrentNode,
            Successors = Successors,
            Predecessors = Predecessors,
            Program = Program
        });
    }

    public void GoBack()
    {
        var previous = _session.Previous();
        if (previous != null)
            LoadFromHistory(previous);
    }

    public void GoBackWithRemove()
    {
        var previous = _session.PreviousWithRemove();
        if (previous != null)
            LoadFromHistory(previous);
    }

    public void GoForward()
    {
        var next = _session.Next();
        if (next != null)
            LoadFromHistory(next);
    }

    public void GoTo(int index)
    {
        _session.GoTo(index);
        var entry = _session.Current();
        if (entry != null)
            LoadFromHistory(entry);
    }

    private async Task LoadFromDatabaseAsync(string lanUid, string linUid, string edgDir, bool useEdg = false)
    {
        var row = await _repository.GetByPKAsync(lanUid, linUid, edgDir);
        if (row != null)
        {
            _session.CurrentId = $"{lanUid}|{linUid}|{edgDir}";
            // Use EDG (LinkedNode) if useEdg is true, otherwise use DTA (SourceNode)
            _session.CurrentNode = useEdg ? row.LinkedNode : row.SourceNode;
            Successors = await _repository.GetSuccessorsAsync(lanUid, linUid, edgDir, useEdg);
            Predecessors = await _repository.GetPredecessorsAsync(lanUid, linUid, edgDir, useEdg);
            Program = await _repository.GetProgramAsync(lanUid);
        }
    }

    private void LoadFromHistory(HistoryEntry entry)
    {
        _session.CurrentId = entry.Id;
        _session.CurrentNode = entry.Node;
        Successors = entry.Successors;
        Predecessors = entry.Predecessors;
        Program = entry.Program;
    }
}
