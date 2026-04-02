using CommunityToolkit.Mvvm.ComponentModel;

namespace MuralDigital.ViewModels;

public partial class MuralItemViewModel : ObservableObject
{
    private readonly Models.MuralItem _model;

    /// <summary>Tracks which URL was last shortened, so we detect changes.</summary>
    private string _lastShortenedUrl = string.Empty;

    public MuralItemViewModel(Models.MuralItem model)
    {
        _model = model;
        _label = model.Label;
        _originalUrl = model.OriginalUrl;
        _shortUrl = model.ShortUrl;

        if (!string.IsNullOrWhiteSpace(_shortUrl))
            _lastShortenedUrl = _originalUrl;
    }

    [ObservableProperty]
    private string _label = string.Empty;

    [ObservableProperty]
    private string _originalUrl = string.Empty;

    [ObservableProperty]
    private string _shortUrl = string.Empty;

    [ObservableProperty]
    private bool _isShortening;

    /// <summary>True when OriginalUrl was changed after the last shortening.</summary>
    public bool NeedsReshortening =>
        !string.IsNullOrWhiteSpace(OriginalUrl)
        && (string.IsNullOrWhiteSpace(ShortUrl) || OriginalUrl != _lastShortenedUrl);

    partial void OnOriginalUrlChanged(string value)
    {
        // If user changed the URL after it was shortened, invalidate the short URL
        if (!string.IsNullOrWhiteSpace(_lastShortenedUrl) && value != _lastShortenedUrl)
        {
            ShortUrl = string.Empty;
        }
    }

    /// <summary>Called after a successful shortening to record the source URL.</summary>
    public void MarkShortened()
    {
        _lastShortenedUrl = OriginalUrl;
    }

    public Models.MuralItem ToModel() => new()
    {
        Id = _model.Id,
        Label = Label,
        OriginalUrl = OriginalUrl,
        ShortUrl = ShortUrl
    };
}
