using System.ComponentModel;
using LignageApp.Models;
using LignageApp.Services;

namespace LignageApp.ViewModels;

public class ConsultationViewModel : INotifyPropertyChanged
{
    private readonly IConsultationService _consultationService;

    public int IdActuel => _consultationService.IdActuel;
    public string NoeudActuel => _consultationService.NoeudActuel;
    public List<LignageDto> Successeurs => _consultationService.Successeurs;
    public List<LignageDto> Predecesseurs => _consultationService.Predecesseurs;
    public ProgrammeDto? Programme => _consultationService.Programme;
    public bool PeutRevenirEnArriere => _consultationService.PeutRevenirEnArriere;
    public List<HistoriqueEntry> Historique => _consultationService.Historique;
    public bool DrawerOuvert { get; private set; }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void Notifier(string? prop = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));

    public ConsultationViewModel(IConsultationService consultationService)
    {
        _consultationService = consultationService;
    }

    public async Task InitialiserParIdAsync(int id)
    {
        await _consultationService.InitialiserParIdAsync(id);
        Notifier();
    }

    public async Task NaviguerVersAsync(int idCible)
    {
        await _consultationService.NaviguerVersAsync(idCible);
        Notifier();
    }

    public void Retour()
    {
        _consultationService.Retour();
        Notifier();
    }

    public void RevenirA(int idCible)
    {
        _consultationService.RevenirA(idCible);
        Notifier();
    }

    public void ToggleHistorique()
    {
        DrawerOuvert = !DrawerOuvert;
        Notifier(nameof(DrawerOuvert));
    }
}
