using LignageApp.Data;
using LignageApp.Models;

namespace LignageApp.Services;

public class ConsultationService : IConsultationService
{
    private readonly ILignageRepository _repository;
    private readonly NavigationSession _session;

    public int IdActuel => _session.IdActuel;
    public string NoeudActuel => _session.NoeudActuel;
    public List<LignageDto> Successeurs { get; private set; } = new();
    public List<LignageDto> Predecesseurs { get; private set; } = new();
    public ProgrammeDto? Programme { get; private set; }
    public bool PeutRevenirEnArriere => _session.PeutRevenirEnArriere;
    public List<HistoriqueEntry> Historique => _session.ObtenirHistorique();

    public ConsultationService(ILignageRepository repository, NavigationSession session)
    {
        _repository = repository;
        _session = session;
    }

    public async Task InitialiserParIdAsync(int id)
    {
        _session.Reinitialiser();
        await ChargerDepuisBaseAsync(id);

        _session.Empiler(new HistoriqueEntry
        {
            Id = id,
            Noeud = _session.NoeudActuel,
            Successeurs = Successeurs,
            Predecesseurs = Predecesseurs,
            Programme = Programme
        });
    }

    public async Task NaviguerVersAsync(int idCible)
    {
        await ChargerDepuisBaseAsync(idCible);

        _session.Empiler(new HistoriqueEntry
        {
            Id = idCible,
            Noeud = _session.NoeudActuel,
            Successeurs = Successeurs,
            Predecesseurs = Predecesseurs,
            Programme = Programme
        });
    }

    public void Retour()
    {
        var precedent = _session.Depiler();
        if (precedent != null)
            ChargerDepuisHistorique(precedent);
    }

    public void RevenirA(int idCible)
    {
        _session.RevenirJusquA(idCible);
        var entree = _session.Consulter();
        if (entree != null)
            ChargerDepuisHistorique(entree);
    }

    private async Task ChargerDepuisBaseAsync(int id)
    {
        var noeud = await _repository.GetNoeudByIdAsync(id);
        if (noeud != null)
        {
            _session.IdActuel = id;
            _session.NoeudActuel = noeud.Noeud;
            Successeurs = await _repository.GetSuccesseursAsync(id);
            Predecesseurs = await _repository.GetPredecesseursAsync(id);
            Programme = await _repository.GetProgrammeAsync(id);
        }
    }

    private void ChargerDepuisHistorique(HistoriqueEntry entree)
    {
        _session.IdActuel = entree.Id;
        _session.NoeudActuel = entree.Noeud;
        Successeurs = entree.Successeurs;
        Predecesseurs = entree.Predecesseurs;
        Programme = entree.Programme;
    }
}
