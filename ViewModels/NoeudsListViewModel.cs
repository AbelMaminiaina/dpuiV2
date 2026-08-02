using System.ComponentModel;
using LignageApp.Models;
using LignageApp.Services;

namespace LignageApp.ViewModels;

public class NoeudsListViewModel : INotifyPropertyChanged
{
    private readonly INoeudsService _noeudsService;

    public List<NoeudDto> Noeuds { get; private set; } = new();
    public bool EnChargement { get; private set; }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void Notifier(string? prop = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));

    public NoeudsListViewModel(INoeudsService noeudsService)
    {
        _noeudsService = noeudsService;
    }

    public async Task ChargerAsync()
    {
        EnChargement = true;
        Notifier(nameof(EnChargement));

        Noeuds = await _noeudsService.GetNoeudsAsync();

        EnChargement = false;
        Notifier(nameof(EnChargement));
        Notifier(nameof(Noeuds));
    }
}
