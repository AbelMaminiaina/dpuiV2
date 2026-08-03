using LignageApp.Models;

namespace LignageApp.Services;

public class NavigationSession
{
    private readonly List<HistoriqueEntry> _historiqueNavigation = new();
    private int _indexActuel = -1;

    public string IdActuel { get; set; } = string.Empty;  // Format: LnaUid_LinUid_EdgDir
    public string NoeudActuel { get; set; } = string.Empty;

    public void Ajouter(HistoriqueEntry entree)
    {
        if (_indexActuel < _historiqueNavigation.Count - 1)
        {
            _historiqueNavigation.RemoveRange(_indexActuel + 1, _historiqueNavigation.Count - _indexActuel - 1);
        }
        _historiqueNavigation.Add(entree);
        _indexActuel = _historiqueNavigation.Count - 1;
    }

    public HistoriqueEntry? Precedent()
    {
        if (_indexActuel > 0)
        {
            _indexActuel--;
            return _historiqueNavigation[_indexActuel];
        }
        return null;
    }

    public HistoriqueEntry? PrecedentAvecSuppression()
    {
        if (_historiqueNavigation.Count > 1)
        {
            _historiqueNavigation.RemoveAt(_historiqueNavigation.Count - 1);
            _indexActuel = _historiqueNavigation.Count - 1;
            return _historiqueNavigation[_indexActuel];
        }
        return null;
    }

    public HistoriqueEntry? Suivant()
    {
        if (_indexActuel < _historiqueNavigation.Count - 1)
        {
            _indexActuel++;
            return _historiqueNavigation[_indexActuel];
        }
        return null;
    }

    public HistoriqueEntry? Consulter() =>
        _indexActuel >= 0 && _indexActuel < _historiqueNavigation.Count
            ? _historiqueNavigation[_indexActuel]
            : null;

    public bool PeutRevenirEnArriere => _indexActuel > 0;
    public bool PeutAllerEnAvant => _indexActuel < _historiqueNavigation.Count - 1;

    public List<HistoriqueEntry> ObtenirHistorique() => _historiqueNavigation.ToList();

    public int IndexActuel => _indexActuel;

    public void AllerA(int index)
    {
        if (index >= 0 && index < _historiqueNavigation.Count)
        {
            _indexActuel = index;
        }
    }

    public void Reinitialiser()
    {
        _historiqueNavigation.Clear();
        _indexActuel = -1;
        IdActuel = string.Empty;
        NoeudActuel = string.Empty;
    }
}
