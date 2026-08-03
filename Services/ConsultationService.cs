using LignageApp.Data;
using LignageApp.Models;

namespace LignageApp.Services;

public class ConsultationService : IConsultationService
{
    private readonly ILignageRepository _repository;
    private readonly NavigationSession _session;

    public string IdActuel => _session.IdActuel;
    public string NoeudActuel => _session.NoeudActuel;
    public List<LignageDto> Successeurs { get; private set; } = new();
    public List<LignageDto> Predecesseurs { get; private set; } = new();
    public ProgrammeDto? Programme { get; private set; }
    public bool PeutRevenirEnArriere => _session.PeutRevenirEnArriere;
    public bool PeutAllerEnAvant => _session.PeutAllerEnAvant;
    public List<HistoriqueEntry> Historique => _session.ObtenirHistorique();
    public int IndexActuel => _session.IndexActuel;

    public ConsultationService(ILignageRepository repository, NavigationSession session)
    {
        _repository = repository;
        _session = session;
    }

    public async Task InitialiserAsync(string lnaUid, string linUid, string edgDir)
    {
        _session.Reinitialiser();
        await ChargerDepuisBaseAsync(lnaUid, linUid, edgDir);

        _session.Ajouter(new HistoriqueEntry
        {
            Id = _session.IdActuel,
            Noeud = _session.NoeudActuel,
            Successeurs = Successeurs,
            Predecesseurs = Predecesseurs,
            Programme = Programme
        });
    }

    public async Task NaviguerVersAsync(string lnaUid, string linUid, string edgDir)
    {
        await ChargerDepuisBaseAsync(lnaUid, linUid, edgDir);

        _session.Ajouter(new HistoriqueEntry
        {
            Id = _session.IdActuel,
            Noeud = _session.NoeudActuel,
            Successeurs = Successeurs,
            Predecesseurs = Predecesseurs,
            Programme = Programme
        });
    }

    public void Retour()
    {
        var precedent = _session.Precedent();
        if (precedent != null)
            ChargerDepuisHistorique(precedent);
    }

    public void RetourAvecSuppression()
    {
        var precedent = _session.PrecedentAvecSuppression();
        if (precedent != null)
            ChargerDepuisHistorique(precedent);
    }

    public void Avancer()
    {
        var suivant = _session.Suivant();
        if (suivant != null)
            ChargerDepuisHistorique(suivant);
    }

    public void AllerA(int index)
    {
        _session.AllerA(index);
        var entree = _session.Consulter();
        if (entree != null)
            ChargerDepuisHistorique(entree);
    }

    private async Task ChargerDepuisBaseAsync(string lnaUid, string linUid, string edgDir)
    {
        var row = await _repository.GetByPKAsync(lnaUid, linUid, edgDir);
        if (row != null)
        {
            _session.IdActuel = $"{lnaUid}|{linUid}|{edgDir}";
            _session.NoeudActuel = row.Noeuds1;
            Successeurs = await _repository.GetSuccesseursAsync(lnaUid, linUid, edgDir);
            Predecesseurs = await _repository.GetPredecesseursAsync(lnaUid, linUid, edgDir);
            Programme = await _repository.GetProgrammeAsync(lnaUid);
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
