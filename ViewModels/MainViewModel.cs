using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MuralDigital.Models;
using MuralDigital.Services;

namespace MuralDigital.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly IMuralDataService _dataService;
    private readonly IUrlShortenerService _urlShortener;
    private readonly IWhatsAppTextGenerator _textGenerator;
    private List<WhatsAppContact> _contacts = [];

    public MainViewModel(
        IMuralDataService dataService,
        IUrlShortenerService urlShortener,
        IWhatsAppTextGenerator textGenerator)
    {
        _dataService = dataService;
        _urlShortener = urlShortener;
        _textGenerator = textGenerator;
    }

    [ObservableProperty]
    private string _congregation = "Cong. Auxiliadora";

    [ObservableProperty]
    private string _headerText = "MURAL ON-LINE";

    [ObservableProperty]
    private string _visitNote = string.Empty;

    [ObservableProperty]
    private string _footerSection = string.Empty;

    [ObservableProperty]
    private string _generatedText = string.Empty;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private Color _statusColor = Colors.Gray;

    public ObservableCollection<MuralGroupViewModel> Groups { get; } = [];

    [RelayCommand]
    private async Task LoadDataAsync()
    {
        IsBusy = true;
        StatusMessage = "⏳ Carregando dados...";
        StatusColor = Colors.SlateBlue;
        try
        {
            var config = await _dataService.LoadAsync();
            Congregation = config.Congregation;
            HeaderText = config.HeaderText;
            VisitNote = config.VisitNote;
            FooterSection = config.FooterSection;

            Groups.Clear();
            foreach (var group in config.Groups.OrderBy(g => g.Order))
            {
                Groups.Add(new MuralGroupViewModel(group, _urlShortener));
            }

            _contacts = config.Contacts ?? [];

            StatusMessage = "✅ Dados carregados com sucesso.";
            StatusColor = Colors.Green;
        }
        catch (Exception ex)
        {
            StatusMessage = $"❌ Erro ao carregar: {ex.Message}";
            StatusColor = Colors.OrangeRed;
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task SaveDataAsync()
    {
        var error = ValidateForSave();
        if (error is not null)
        {
            StatusMessage = error;
            StatusColor = Colors.OrangeRed;
            await ShowAlertAsync("Campos Obrigat\u00f3rios", error);
            return;
        }

        IsBusy = true;
        StatusMessage = "💾 Salvando...";
        StatusColor = Colors.SlateBlue;
        try
        {
            var config = BuildConfig();
            await _dataService.SaveAsync(config);
            StatusMessage = "✅ Dados salvos com sucesso!";
            StatusColor = Colors.Green;
        }
        catch (Exception ex)
        {
            StatusMessage = $"❌ Erro ao salvar: {ex.Message}";
            StatusColor = Colors.OrangeRed;
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task ShortenAllUrlsAsync()
    {
        var hasUrls = Groups.Any(g => g.Items.Any(i => !string.IsNullOrWhiteSpace(i.OriginalUrl)));
        if (!hasUrls)
        {
            StatusMessage = "⚠️ Nenhuma URL encontrada para encurtar. Preencha os campos de link primeiro.";
            StatusColor = Colors.OrangeRed;
            return;
        }

        IsBusy = true;
        StatusMessage = "🔗 Encurtando URLs...";
        StatusColor = Colors.SlateBlue;
        try
        {
            foreach (var group in Groups)
            {
                await group.ShortenAllUrlsCommand.ExecuteAsync(null);
            }

            StatusMessage = "✅ Todas as URLs foram encurtadas com sucesso!";
            StatusColor = Colors.Green;
        }
        catch (Exception ex)
        {
            StatusMessage = $"❌ Erro ao encurtar: {ex.Message}";
            StatusColor = Colors.OrangeRed;
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task PreviewTextAsync()
    {
        var config = BuildConfig();
        var hasContent = (config.Groups ?? [])
            .Where(g => g.Items is not null)
            .Any(g => g.Items!.Any(i =>
                !string.IsNullOrWhiteSpace(i.ShortUrl) || !string.IsNullOrWhiteSpace(i.OriginalUrl)));

        if (!hasContent)
        {
            StatusMessage = "⚠️ Nenhum conteúdo para visualizar. Preencha pelo menos um link.";
            StatusColor = Colors.OrangeRed;
            return;
        }

        try
        {
            var navParams = new ShellNavigationQueryParameters { ["config"] = config };
            await Shell.Current.GoToAsync("preview", navParams);
        }
        catch (Exception ex)
        {
            StatusMessage = $"❌ Erro ao abrir preview: {ex.Message}";
            StatusColor = Colors.OrangeRed;
        }
    }

    [RelayCommand]
    private async Task GenerateTextAsync()
    {
        var error = ValidateRequiredFields();
        if (error is not null)
        {
            StatusMessage = error;
            StatusColor = Colors.OrangeRed;
            await ShowAlertAsync("Campos Obrigatórios", error);
            return;
        }

        IsBusy = true;
        StatusColor = Colors.SlateBlue;

        try
        {
            // Re-encurtar URLs que foram alteradas
            var dirtyCount = Groups.Sum(g => g.Items.Count(i => i.NeedsReshortening));
            if (dirtyCount > 0)
            {
                StatusMessage = $"🔗 Encurtando {dirtyCount} URL(s) alterada(s)...";
                foreach (var group in Groups)
                {
                    await group.ShortenAllUrlsCommand.ExecuteAsync(null);
                }
            }

            StatusMessage = "✅ URLs prontas! Abrindo pré-visualização...";
            StatusColor = Colors.Green;
        }
        catch (Exception ex)
        {
            StatusMessage = $"❌ Erro ao encurtar: {ex.Message}";
            StatusColor = Colors.OrangeRed;
            return;
        }
        finally
        {
            IsBusy = false;
        }

        // Navegar para PreviewPage com dados atualizados
        try
        {
            var config = BuildConfig();
            var navParams = new ShellNavigationQueryParameters { ["config"] = config };
            await Shell.Current.GoToAsync("preview", navParams);
        }
        catch (Exception ex)
        {
            StatusMessage = $"❌ Erro ao abrir preview: {ex.Message}";
            StatusColor = Colors.OrangeRed;
        }
    }

    [RelayCommand]
    private async Task CopyToClipboardAsync()
    {
        var error = ValidateRequiredFields();
        if (error is not null)
        {
            StatusMessage = error;
            StatusColor = Colors.OrangeRed;
            await ShowAlertAsync("Campos Obrigat\u00f3rios", error);
            return;
        }

        IsBusy = true;
        StatusColor = Colors.SlateBlue;

        try
        {
            // Re-encurtar URLs alteradas
            var dirtyCount = Groups.Sum(g => g.Items.Count(i => i.NeedsReshortening));
            if (dirtyCount > 0)
            {
                StatusMessage = $"\ud83d\udd17 Encurtando {dirtyCount} URL(s)...";
                foreach (var group in Groups)
                {
                    await group.ShortenAllUrlsCommand.ExecuteAsync(null);
                }
            }

            // Gerar texto com estilo padr\u00e3o (Cl\u00e1ssico)
            var config = BuildConfig();
            var text = _textGenerator.Generate(config, WhatsAppStyle.Classico);
            GeneratedText = text;

            // Copiar para \u00e1rea de transfer\u00eancia
            await Clipboard.Default.SetTextAsync(text);

            // Auto-salvar
            await _dataService.SaveAsync(config);

            StatusMessage = "\u2705 Texto gerado e copiado! Cole com Ctrl+V.";
            StatusColor = Colors.Green;
        }
        catch (Exception ex)
        {
            StatusMessage = $"\u274c Erro: {ex.Message}";
            StatusColor = Colors.OrangeRed;
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void AddGroup()
    {
        var newGroup = new MuralGroup
        {
            Emoji = "⚙️",
            Title = $"Novo Grupo {Groups.Count + 1}",
            Order = Groups.Count + 1,
            Items = [new MuralItem { Label = "Item 1" }]
        };
        Groups.Add(new MuralGroupViewModel(newGroup, _urlShortener));
        StatusMessage = $"✅ Grupo '{newGroup.Title}' adicionado.";
        StatusColor = Colors.Green;
    }

    [RelayCommand]
    private void RemoveGroup(MuralGroupViewModel group)
    {
        var title = group.Title;
        Groups.Remove(group);
        StatusMessage = $"🗑️ Grupo '{title}' removido.";
        StatusColor = Colors.Orange;
    }

    private string? ValidateRequiredFields()
    {
        var missing = new List<string>();

        if (string.IsNullOrWhiteSpace(Congregation))
            missing.Add("'Congregação'");
        if (string.IsNullOrWhiteSpace(HeaderText))
            missing.Add("'Título'");
        if (Groups.Count == 0)
        {
            missing.Add("Adicione pelo menos um Grupo");
            return $"Preencha os seguintes campos:\n• {string.Join("\n• ", missing)}";
        }

        bool hasAtLeastOneUrl = false;
        for (int i = 0; i < Groups.Count; i++)
        {
            var group = Groups[i];
            if (string.IsNullOrWhiteSpace(group.Title))
                missing.Add($"'Título' do grupo na posição {i + 1}");

            for (int j = 0; j < group.Items.Count; j++)
            {
                var item = group.Items[j];
                var groupName = !string.IsNullOrWhiteSpace(group.Title) ? group.Title : $"#{i + 1}";
                var hasUrl = !string.IsNullOrWhiteSpace(item.OriginalUrl) || !string.IsNullOrWhiteSpace(item.ShortUrl);

                if (hasUrl)
                    hasAtLeastOneUrl = true;

                // Item sem link = ignorado (não será gerado)
                // Item com link mas sem título = erro
                if (hasUrl && string.IsNullOrWhiteSpace(item.Label))
                    missing.Add($"'Título/Label' do item {j + 1} no grupo '{groupName}' (tem link mas falta o título)");
            }
        }

        if (!hasAtLeastOneUrl)
            missing.Add("Preencha pelo menos um link em algum item para gerar o mural");

        return missing.Count > 0
            ? $"Preencha os seguintes campos:\n• {string.Join("\n• ", missing)}"
            : null;
    }

    private string? ValidateForSave()
    {
        var missing = new List<string>();

        if (string.IsNullOrWhiteSpace(Congregation))
            missing.Add("'Congrega\u00e7\u00e3o'");
        if (string.IsNullOrWhiteSpace(HeaderText))
            missing.Add("'T\u00edtulo'");

        return missing.Count > 0
            ? $"Preencha os seguintes campos para salvar:\n\u2022 {string.Join("\n\u2022 ", missing)}"
            : null;
    }

    private static async Task ShowAlertAsync(string title, string message)
    {
        var page = Application.Current?.Windows.FirstOrDefault()?.Page;
        if (page is not null)
            await page.DisplayAlert(title, message, "OK");
    }

    private MuralConfig BuildConfig() => new()
    {
        Congregation = Congregation,
        HeaderEmoji = "🚨",
        HeaderText = HeaderText,
        VisitNote = VisitNote,
        FooterSection = FooterSection,
        Groups = Groups.Select(g => g.ToModel()).ToList(),
        Contacts = _contacts ?? []
    };
}
