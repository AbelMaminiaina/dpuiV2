using System.ComponentModel;
using LignageApp.Models;
using LignageApp.Services;

namespace LignageApp.ViewModels;

public class NodesListViewModel : INotifyPropertyChanged
{
    private readonly INodesService _nodesService;

    public List<NodeDto> Nodes { get; private set; } = new();
    public bool IsLoading { get; private set; }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void Notify(string? prop = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));

    public NodesListViewModel(INodesService nodesService)
    {
        _nodesService = nodesService;
    }

    public async Task LoadAsync()
    {
        IsLoading = true;
        Notify(nameof(IsLoading));

        Nodes = await _nodesService.GetNodesAsync();

        IsLoading = false;
        Notify(nameof(IsLoading));
        Notify(nameof(Nodes));
    }
}
