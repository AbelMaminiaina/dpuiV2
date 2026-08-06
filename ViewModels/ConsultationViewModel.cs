using System.ComponentModel;
using LignageApp.Models;
using LignageApp.Services;

namespace LignageApp.ViewModels;

public class ConsultationViewModel : INotifyPropertyChanged
{
    private readonly IConsultationService _consultationService;

    public string CurrentId => _consultationService.CurrentId;
    public string CurrentNode => _consultationService.CurrentNode;
    public List<LineageDto> Successors => _consultationService.Successors;
    public List<LineageDto> Predecessors => _consultationService.Predecessors;
    public ProgramDto? Program => _consultationService.Program;
    public bool CanGoBack => _consultationService.CanGoBack;
    public bool CanGoForward => _consultationService.CanGoForward;
    public List<HistoryEntry> History => _consultationService.History;
    public int CurrentIndex => _consultationService.CurrentIndex;
    public bool DrawerOpen { get; private set; }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void Notify(string? prop = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));

    public ConsultationViewModel(IConsultationService consultationService)
    {
        _consultationService = consultationService;
    }

    public async Task InitializeAsync(string lanUid, string linUid, string edgDir)
    {
        await _consultationService.InitializeAsync(lanUid, linUid, edgDir);
        Notify();
    }

    public async Task NavigateToAsync(string id)
    {
        // id format: lanUid|linUid|edgDir|nodeType (D=DTA, E=EDG)
        var parts = id?.Split('|');
        if (parts != null && parts.Length >= 3)
        {
            var lanUid = Uri.UnescapeDataString(parts[0]);
            var linUid = Uri.UnescapeDataString(parts[1]);
            var edgDir = parts[2];
            var useEdg = parts.Length >= 4 && parts[3] == "E";
            await _consultationService.NavigateToAsync(lanUid, linUid, edgDir, useEdg);
            Notify();
        }
    }

    public void GoBack()
    {
        _consultationService.GoBack();
        Notify();
    }

    public void GoBackWithRemove()
    {
        _consultationService.GoBackWithRemove();
        Notify();
    }

    public void GoForward()
    {
        _consultationService.GoForward();
        Notify();
    }

    public void GoTo(int index)
    {
        _consultationService.GoTo(index);
        Notify();
    }

    public void ToggleHistory()
    {
        DrawerOpen = !DrawerOpen;
        Notify(nameof(DrawerOpen));
    }
}
