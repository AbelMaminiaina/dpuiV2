using LignageApp.Models;

namespace LignageApp.Services;

public class NavigationSession
{
    private readonly Stack<HistoriqueEntry> _historiqueNavigation = new();

    public int IdActuel { get; set; }
    public string NoeudActuel { get; set; }

    public void Empiler(HistoriqueEntry entree) => _historiqueNavigation.Push(entree);

    public HistoriqueEntry Depiler() =>
        _historiqueNavigation.Count > 0 ? _historiqueNavigation.Pop() : null;

    public HistoriqueEntry? Consulter() =>
        _historiqueNavigation.Count > 0 ? _historiqueNavigation.Peek() : null;

    public bool PeutRevenirEnArriere => _historiqueNavigation.Count > 0;

    public List<HistoriqueEntry> ObtenirHistorique() =>
        _historiqueNavigation.Reverse().ToList();

    public void RevenirJusquA(int id)
    {
        while (_historiqueNavigation.Count > 0 && _historiqueNavigation.Peek().Id != id)
            _historiqueNavigation.Pop();
    }

    public void Reinitialiser() => _historiqueNavigation.Clear();
}
