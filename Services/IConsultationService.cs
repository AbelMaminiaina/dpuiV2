using LignageApp.Models;

namespace LignageApp.Services;

public interface IConsultationService
{
    string IdActuel { get; }
    string NoeudActuel { get; }
    List<LignageDto> Successeurs { get; }
    List<LignageDto> Predecesseurs { get; }
    ProgrammeDto? Programme { get; }
    bool PeutRevenirEnArriere { get; }
    bool PeutAllerEnAvant { get; }
    List<HistoriqueEntry> Historique { get; }
    int IndexActuel { get; }

    Task InitialiserAsync(string lnaUid, string linUid, string edgDir);
    Task NaviguerVersAsync(string lnaUid, string linUid, string edgDir);
    void Retour();
    void RetourAvecSuppression();
    void Avancer();
    void AllerA(int index);
}
