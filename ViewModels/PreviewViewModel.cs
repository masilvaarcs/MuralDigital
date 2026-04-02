using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MuralDigital.Models;
using MuralDigital.Services;

namespace MuralDigital.ViewModels;

public partial class PreviewViewModel : ObservableObject, IQueryAttributable
{
    private readonly IWhatsAppTextGenerator _textGenerator;
    private readonly IUrlShortenerService _urlShortener;
    private readonly IMuralDataService _dataService;
    private MuralConfig _config = new();

    public PreviewViewModel(
        IWhatsAppTextGenerator textGenerator,
        IUrlShortenerService urlShortener,
        IMuralDataService dataService)
    {
        _textGenerator = textGenerator;
        _urlShortener = urlShortener;
        _dataService = dataService;

        var styles = _textGenerator.GetAvailableStyles();
        foreach (var s in styles)
            AvailableStyles.Add(new StyleOption(s));

        _selectedStyle = AvailableStyles[0];
    }

    public ObservableCollection<StyleOption> AvailableStyles { get; } = [];
    public ObservableCollection<ContactViewModel> Contacts { get; } = [];

    [ObservableProperty]
    private StyleOption _selectedStyle;

    [ObservableProperty]
    private string _previewText = string.Empty;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private Color _statusColor = Colors.Gray;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string _newContactName = string.Empty;

    [ObservableProperty]
    private string _newContactPhone = string.Empty;

    public string? ConfirmedText { get; private set; }
    public WhatsAppStyle ConfirmedStyle { get; private set; }
    public bool WasConfirmed { get; private set; }

    partial void OnSelectedStyleChanged(StyleOption value)
    {
        RefreshPreview();
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("config", out var configObj) && configObj is MuralConfig config)
        {
            _config = config;

            // Garantir que listas não são null (JSON antigo)
            _config.Groups ??= [];
            _config.Contacts ??= [];

            Contacts.Clear();
            foreach (var c in _config.Contacts)
                Contacts.Add(new ContactViewModel(c));

            // Ensure at least the default is selected
            EnsureAtLeastOneSelected();
            RefreshPreview();
        }
    }

    private void RefreshPreview()
    {
        if (SelectedStyle is null || _config is null) return;
        try
        {
            PreviewText = _textGenerator.Generate(_config, SelectedStyle.Style);
        }
        catch (Exception ex)
        {
            PreviewText = $"Erro ao gerar preview: {ex.Message}";
        }
    }

    private void EnsureAtLeastOneSelected()
    {
        if (Contacts.Any(c => c.IsSelected)) return;
        var defaultContact = Contacts.FirstOrDefault(c => c.IsDefault) ?? Contacts.FirstOrDefault();
        if (defaultContact is not null)
            defaultContact.IsSelected = true;
    }

    [RelayCommand]
    private void AddContact()
    {
        var phone = NormalizePhone(NewContactPhone);
        if (string.IsNullOrWhiteSpace(NewContactName) || string.IsNullOrWhiteSpace(phone))
        {
            StatusMessage = "⚠️ Informe nome e telefone do contato.";
            StatusColor = Colors.OrangeRed;
            return;
        }

        if (Contacts.Any(c => c.Phone == phone))
        {
            StatusMessage = $"⚠️ O número {phone} já está na lista.";
            StatusColor = Colors.OrangeRed;
            return;
        }

        var contact = new ContactViewModel(new WhatsAppContact
        {
            Name = NewContactName.Trim(),
            Phone = phone,
            IsDefault = false,
            IsSelected = true
        });
        Contacts.Add(contact);

        NewContactName = string.Empty;
        NewContactPhone = string.Empty;

        StatusMessage = $"✅ Contato '{contact.Name}' adicionado.";
        StatusColor = Colors.Green;
    }

    [RelayCommand]
    private void RemoveContact(ContactViewModel contact)
    {
        if (contact.IsDefault)
        {
            StatusMessage = "⚠️ Não é possível remover o contato padrão.";
            StatusColor = Colors.OrangeRed;
            return;
        }

        Contacts.Remove(contact);
        EnsureAtLeastOneSelected();
        StatusMessage = $"🗑️ Contato '{contact.Name}' removido.";
        StatusColor = Colors.Orange;
    }

    [RelayCommand]
    private async Task ConfirmStyleAsync()
    {
        // Validate at least one contact selected
        var selectedContacts = Contacts.Where(c => c.IsSelected).ToList();
        if (selectedContacts.Count == 0)
        {
            StatusMessage = "⚠️ Selecione pelo menos um contato para enviar.";
            StatusColor = Colors.OrangeRed;
            return;
        }

        IsBusy = true;
        StatusMessage = "⏳ Preparando texto final...";
        StatusColor = Colors.SlateBlue;

        try
        {
            // Re-shorten dirty URLs
            foreach (var group in _config.Groups)
            {
                foreach (var item in group.Items ?? [])
                {
                    var url = item.OriginalUrl;
                    if (!string.IsNullOrWhiteSpace(url) && string.IsNullOrWhiteSpace(item.ShortUrl))
                    {
                        item.ShortUrl = await _urlShortener.ShortenAsync(url, item.Label);
                    }
                }
            }

            // Generate final text (mesmo gerador do preview)
            ConfirmedText = _textGenerator.Generate(_config, SelectedStyle.Style);
            ConfirmedStyle = SelectedStyle.Style;
            WasConfirmed = true;

            // Copy to clipboard FIRST — this is the authoritative text
            await Clipboard.Default.SetTextAsync(ConfirmedText);

            // Save contacts back into config and persist
            _config.Contacts = Contacts.Select(c => c.ToModel()).ToList();
            await _dataService.SaveAsync(_config);

            // Send to each selected contact
            var sentNames = new List<string>();
            var failedNames = new List<string>();

            foreach (var contact in selectedContacts)
            {
                var sent = await TrySendWhatsAppAsync(contact.Phone, ConfirmedText);
                if (!sent)
                {
                    // Retry with '9' inserted after area code
                    var phoneWith9 = InsertNineAfterAreaCode(contact.Phone);
                    if (phoneWith9 != contact.Phone)
                        sent = await TrySendWhatsAppAsync(phoneWith9, ConfirmedText);
                }

                if (sent)
                    sentNames.Add(contact.Name);
                else
                    failedNames.Add(contact.Name);
            }

            // Build result message
            var parts = new List<string> { "📋 Texto copiado para a área de transferência." };
            if (sentNames.Count > 0)
                parts.Add($"✅ WhatsApp aberto para: {string.Join(", ", sentNames)}.");
            if (failedNames.Count > 0)
                parts.Add($"⚠️ Falha ao abrir para: {string.Join(", ", failedNames)}.");
            parts.Add("Cole o texto (Ctrl+V) na conversa e envie.");

            StatusMessage = string.Join("\n", parts);
            StatusColor = failedNames.Count > 0 ? Colors.Orange : Colors.Green;

            // Longer delay so user reads the instruction
            await Task.Delay(3000);
            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            StatusMessage = $"❌ Erro: {ex.Message}";
            StatusColor = Colors.OrangeRed;
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task GoBackAsync()
    {
        WasConfirmed = false;
        await Shell.Current.GoToAsync("..");
    }

    [RelayCommand]
    private async Task CopyOnlyAsync()
    {
        try
        {
            // Re-shorten if needed
            foreach (var group in _config.Groups)
            {
                foreach (var item in group.Items ?? [])
                {
                    if (!string.IsNullOrWhiteSpace(item.OriginalUrl) && string.IsNullOrWhiteSpace(item.ShortUrl))
                        item.ShortUrl = await _urlShortener.ShortenAsync(item.OriginalUrl, item.Label);
                }
            }

            var text = _textGenerator.Generate(_config, SelectedStyle.Style);
            await Clipboard.Default.SetTextAsync(text);

            StatusMessage = "✅ Texto copiado! Cole com Ctrl+V onde quiser.";
            StatusColor = Colors.Green;
        }
        catch (Exception ex)
        {
            StatusMessage = $"❌ Erro ao copiar: {ex.Message}";
            StatusColor = Colors.OrangeRed;
        }
    }

    /// <summary>
    /// Opens WhatsApp chat for the given phone number.
    /// Text is already in the clipboard — user pastes with Ctrl+V.
    /// </summary>
    private static async Task<bool> TrySendWhatsAppAsync(string phone, string _)
    {
        try
        {
            var url = $"https://wa.me/{phone}";
            return await Launcher.Default.OpenAsync(new Uri(url));
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Inserts '9' after area code for BR mobile numbers.
    /// Input: "5551XXXXXXXX" (12 digits) -> "55519XXXXXXXX" (13 digits)
    /// If already 13 digits, returns as-is.
    /// </summary>
    private static string InsertNineAfterAreaCode(string phone)
    {
        // BR format: CC(2) + DDD(2) + number(8 or 9)
        if (phone.Length == 12 && phone.StartsWith("55"))
            return phone[..4] + "9" + phone[4..];
        return phone;
    }

    /// <summary>
    /// Strips all non-digit characters from a phone string.
    /// </summary>
    private static string NormalizePhone(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return string.Empty;
        return new string(input.Where(char.IsDigit).ToArray());
    }
}

public class StyleOption
{
    public StyleOption(WhatsAppStyle style)
    {
        Style = style;
        Name = WhatsAppStyleInfo.GetName(style);
        Description = WhatsAppStyleInfo.GetDescription(style);
    }

    public WhatsAppStyle Style { get; }
    public string Name { get; }
    public string Description { get; }

    public override string ToString() => Name;
}
