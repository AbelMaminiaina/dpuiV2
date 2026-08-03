using System.ComponentModel;
using LignageApp.Models;
using LignageApp.Services;

namespace LignageApp.ViewModels;

public class ConsultationViewModel : INotifyPropertyChanged
{
    private readonly IConsultationService _consultationService;

    public string IdActuel => _consultationService.IdActuel;
    public string NoeudActuel => _consultationService.NoeudActuel;
    public List<LignageDto> Successeurs => _consultationService.Successeurs;
    public List<LignageDto> Predecesseurs => _consultationService.Predecesseurs;
    public ProgrammeDto? Programme => _consultationService.Programme;
    public bool PeutRevenirEnArriere => _consultationService.PeutRevenirEnArriere;
    public bool PeutAllerEnAvant => _consultationService.PeutAllerEnAvant;
    public List<HistoriqueEntry> Historique => _consultationService.Historique;
    public int IndexActuel => _consultationService.IndexActuel;
    public bool DrawerOuvert { get; private set; }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void Notifier(string? prop = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));

    public ConsultationViewModel(IConsultationService consultationService)
    {
        _consultationService = consultationService;
    }

    public async Task InitialiserAsync(string lnaUid, string linUid, string edgDir)
    {
        await _consultationService.InitialiserAsync(lnaUid, linUid, edgDir);
        Notifier();
    }

    public async Task NaviguerVersAsync(string id)
    {
        // id est au format: lnaUid|linUid|edgDir
        var pk = LignagePK.Parse(id);
        if (pk != null)
        {
            await _consultationService.NaviguerVersAsync(pk.LnaUid, pk.LinUid, pk.EdgDir);
            Notifier();
        }
    }

    public void Retour()
    {
        _consultationService.Retour();
        Notifier();
    }

    public void RetourAvecSuppression()
    {
        _consultationService.RetourAvecSuppression();
        Notifier();
    }

    public void Avancer()
    {
        _consultationService.Avancer();
        Notifier();
    }

    public void AllerA(int index)
    {
        _consultationService.AllerA(index);
        Notifier();
    }

    public void ToggleHistorique()
    {
        DrawerOuvert = !DrawerOuvert;
        Notifier(nameof(DrawerOuvert));
    }
}
