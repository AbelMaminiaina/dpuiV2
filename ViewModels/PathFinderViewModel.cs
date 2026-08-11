using LignageApp.Data;
using LignageApp.Models;

namespace LignageApp.ViewModels;

public class PathFinderViewModel
{
    private readonly ILignageRepository _repository;

    public string StartSearchTerm { get; set; } = string.Empty;
    public string EndSearchTerm { get; set; } = string.Empty;
    public List<NodeDto> StartSearchResults { get; private set; } = new();
    public List<NodeDto> EndSearchResults { get; private set; } = new();
    public NodeDto? SelectedStartNode { get; set; }
    public NodeDto? SelectedEndNode { get; set; }
    public PathResult? Result { get; private set; }
    public bool IsSearching { get; private set; }
    public string? ErrorMessage { get; private set; }

    public event EventHandler? PropertyChanged;

    public PathFinderViewModel(ILignageRepository repository)
    {
        _repository = repository;
    }

    public async Task SearchStartNodesAsync()
    {
        if (string.IsNullOrWhiteSpace(StartSearchTerm) || StartSearchTerm.Length < 2)
        {
            StartSearchResults = new();
            return;
        }

        StartSearchResults = await _repository.SearchNodesAsync(StartSearchTerm, 20);
        PropertyChanged?.Invoke(this, EventArgs.Empty);
    }

    public async Task SearchEndNodesAsync()
    {
        if (string.IsNullOrWhiteSpace(EndSearchTerm) || EndSearchTerm.Length < 2)
        {
            EndSearchResults = new();
            return;
        }

        EndSearchResults = await _repository.SearchNodesAsync(EndSearchTerm, 20);
        PropertyChanged?.Invoke(this, EventArgs.Empty);
    }

    public void SelectStartNode(NodeDto node)
    {
        SelectedStartNode = node;
        StartSearchTerm = node.Node;
        StartSearchResults = new();
        PropertyChanged?.Invoke(this, EventArgs.Empty);
    }

    public void SelectEndNode(NodeDto node)
    {
        SelectedEndNode = node;
        EndSearchTerm = node.Node;
        EndSearchResults = new();
        PropertyChanged?.Invoke(this, EventArgs.Empty);
    }

    public async Task FindPathAsync()
    {
        ErrorMessage = null;
        Result = null;

        if (SelectedStartNode == null)
        {
            ErrorMessage = "Please select a start node";
            PropertyChanged?.Invoke(this, EventArgs.Empty);
            return;
        }

        if (SelectedEndNode == null)
        {
            ErrorMessage = "Please select an end node";
            PropertyChanged?.Invoke(this, EventArgs.Empty);
            return;
        }

        if (SelectedStartNode.Id == SelectedEndNode.Id)
        {
            ErrorMessage = "Start and end nodes must be different";
            PropertyChanged?.Invoke(this, EventArgs.Empty);
            return;
        }

        IsSearching = true;
        PropertyChanged?.Invoke(this, EventArgs.Empty);

        try
        {
            Result = await _repository.FindPathAsync(SelectedStartNode.Id, SelectedEndNode.Id, 20);

            if (!Result.PathExists)
            {
                ErrorMessage = $"No path found between '{SelectedStartNode.Node}' and '{SelectedEndNode.Node}' (max depth: 20)";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error: {ex.Message}";
        }
        finally
        {
            IsSearching = false;
            PropertyChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public void Reset()
    {
        StartSearchTerm = string.Empty;
        EndSearchTerm = string.Empty;
        StartSearchResults = new();
        EndSearchResults = new();
        SelectedStartNode = null;
        SelectedEndNode = null;
        Result = null;
        ErrorMessage = null;
        IsSearching = false;
        PropertyChanged?.Invoke(this, EventArgs.Empty);
    }
}
