using LignageApp.Models;

namespace LignageApp.Services;

public class NavigationSession
{
    private readonly List<HistoryEntry> _navigationHistory = new();
    private int _currentIndex = -1;

    public string CurrentId { get; set; } = string.Empty;  // Format: LanUid|LinUid|EdgDir
    public string CurrentNode { get; set; } = string.Empty;

    public void Add(HistoryEntry entry)
    {
        if (_currentIndex < _navigationHistory.Count - 1)
        {
            _navigationHistory.RemoveRange(_currentIndex + 1, _navigationHistory.Count - _currentIndex - 1);
        }
        _navigationHistory.Add(entry);
        _currentIndex = _navigationHistory.Count - 1;
    }

    public HistoryEntry? Previous()
    {
        if (_currentIndex > 0)
        {
            _currentIndex--;
            return _navigationHistory[_currentIndex];
        }
        return null;
    }

    public HistoryEntry? PreviousWithRemove()
    {
        if (_navigationHistory.Count > 1)
        {
            _navigationHistory.RemoveAt(_navigationHistory.Count - 1);
            _currentIndex = _navigationHistory.Count - 1;
            return _navigationHistory[_currentIndex];
        }
        return null;
    }

    public HistoryEntry? Next()
    {
        if (_currentIndex < _navigationHistory.Count - 1)
        {
            _currentIndex++;
            return _navigationHistory[_currentIndex];
        }
        return null;
    }

    public HistoryEntry? Current() =>
        _currentIndex >= 0 && _currentIndex < _navigationHistory.Count
            ? _navigationHistory[_currentIndex]
            : null;

    public bool CanGoBack => _currentIndex > 0;
    public bool CanGoForward => _currentIndex < _navigationHistory.Count - 1;

    public List<HistoryEntry> GetHistory() => _navigationHistory.ToList();

    public int CurrentIndex => _currentIndex;

    public void GoTo(int index)
    {
        if (index >= 0 && index < _navigationHistory.Count)
        {
            _currentIndex = index;
        }
    }

    public void Reset()
    {
        _navigationHistory.Clear();
        _currentIndex = -1;
        CurrentId = string.Empty;
        CurrentNode = string.Empty;
    }
}
