using LignageApp.Models;

namespace LignageApp.Services;

public interface IConsultationService
{
    int IdActuel { get; }
    string NoeudActuel { get; }
    List<LignageDto> Successeurs { get; }
    List<LignageDto> Predecesseurs { get; }
    ProgrammeDto? Programme { get; }
    bool PeutRevenirEnArriere { get; }
    List<HistoriqueEntry> Historique { get; }

    Task InitialiserParIdAsync(int id);
    Task NaviguerVersAsync(int idCible);
    void Retour();
    void RevenirA(int idCible);
}
