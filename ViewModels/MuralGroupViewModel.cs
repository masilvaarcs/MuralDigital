using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MuralDigital.Services;

namespace MuralDigital.ViewModels;

public partial class MuralGroupViewModel : ObservableObject
{
    private readonly Models.MuralGroup _model;
    private readonly IUrlShortenerService _urlShortener;

    public MuralGroupViewModel(Models.MuralGroup model, IUrlShortenerService urlShortener)
    {
        _model = model;
        _urlShortener = urlShortener;
        _emoji = model.Emoji;
        _title = model.Title;
        _subtitle = model.Subtitle;

        Items = new ObservableCollection<MuralItemViewModel>(
            model.Items.Select(i => new MuralItemViewModel(i)));
    }

    [ObservableProperty]
    private string _emoji = string.Empty;

    [ObservableProperty]
    private string _title = string.Empty;

    [ObservableProperty]
    private string _subtitle = string.Empty;

    public ObservableCollection<MuralItemViewModel> Items { get; }

    [RelayCommand]
    private async Task ShortenUrlAsync(MuralItemViewModel item)
    {
        if (string.IsNullOrWhiteSpace(item.OriginalUrl))
            return;

        item.IsShortening = true;
        try
        {
            item.ShortUrl = await _urlShortener.ShortenAsync(item.OriginalUrl, item.Label);
            item.MarkShortened();
        }
        finally
        {
            item.IsShortening = false;
        }
    }

    [RelayCommand]
    private async Task ShortenAllUrlsAsync()
    {
        foreach (var item in Items)
        {
            if (!string.IsNullOrWhiteSpace(item.OriginalUrl) && item.NeedsReshortening)
            {
                await ShortenUrlAsync(item);
            }
        }
    }

    [RelayCommand]
    private void AddItem()
    {
        var newModel = new Models.MuralItem { Label = $"Item {Items.Count + 1}" };
        Items.Add(new MuralItemViewModel(newModel));
    }

    [RelayCommand]
    private void RemoveItem(MuralItemViewModel item)
    {
        Items.Remove(item);
    }

    public Models.MuralGroup ToModel() => new()
    {
        Id = _model.Id,
        Emoji = Emoji,
        Title = Title,
        Subtitle = Subtitle,
        Order = _model.Order,
        Items = Items.Select(i => i.ToModel()).ToList()
    };
}
