using System.Text.Json;
using MuralDigital.Models;

namespace MuralDigital.Services;

public interface IMuralDataService
{
    Task<MuralConfig> LoadAsync();
    Task SaveAsync(MuralConfig config);
}

public class MuralDataService : IMuralDataService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    private static string FilePath =>
        Path.Combine(FileSystem.AppDataDirectory, "mural_config.json");

    public async Task<MuralConfig> LoadAsync()
    {
        if (!File.Exists(FilePath))
            return CreateDefault();

        try
        {
            var json = await File.ReadAllTextAsync(FilePath);
            var config = JsonSerializer.Deserialize<MuralConfig>(json, JsonOptions) ?? CreateDefault();

            // Garantir que contatos existam (JSON salvo antes dessa feature)
            if (config.Contacts is null || config.Contacts.Count == 0)
            {
                config.Contacts = CreateDefaultContacts();
            }

            return config;
        }
        catch
        {
            return CreateDefault();
        }
    }

    public async Task SaveAsync(MuralConfig config)
    {
        var json = JsonSerializer.Serialize(config, JsonOptions);
        await File.WriteAllTextAsync(FilePath, json);
    }

    private static MuralConfig CreateDefault()
    {
        return new MuralConfig
        {
            Congregation = "Cong. Auxiliadora",
            HeaderEmoji = "🚨",
            HeaderText = "MURAL ON-LINE",
            Contacts = CreateDefaultContacts(),
            Groups =
            [
                new MuralGroup
                {
                    Emoji = "⚙️",
                    Title = "Programação Arranjo de Campo",
                    Order = 1,
                    Items =
                    [
                        new MuralItem { Label = "Atual (Mês/Ano)" },
                        new MuralItem { Label = "Próximo 1" },
                        new MuralItem { Label = "Próximo 2" },
                        new MuralItem { Label = "Próximo 3" }
                    ]
                },
                new MuralGroup
                {
                    Emoji = "⚙️",
                    Title = "Programação Mecânica das Reuniões",
                    Order = 2,
                    Items =
                    [
                        new MuralItem { Label = "Período atual" }
                    ]
                },
                new MuralGroup
                {
                    Emoji = "⚙️",
                    Title = "Programação Reunião - Quarta-feira",
                    Subtitle = "Mês/Ano",
                    Order = 3,
                    Items =
                    [
                        new MuralItem { Label = "Semana 1" },
                        new MuralItem { Label = "Semana 2" },
                        new MuralItem { Label = "Semana 3" },
                        new MuralItem { Label = "Semana 4" }
                    ]
                },
                new MuralGroup
                {
                    Emoji = "⚙️",
                    Title = "Limpeza e Jardinagem",
                    Order = 4,
                    Items =
                    [
                        new MuralItem { Label = "Período atual" }
                    ]
                }
            ]
        };
    }

    private static List<WhatsAppContact> CreateDefaultContacts() =>
    [
        new WhatsAppContact
        {
            Name = "Marcos Silva",
            Phone = "5551984228067",
            IsDefault = true,
            IsSelected = true
        },
        new WhatsAppContact
        {
            Name = "Charlie Silva",
            Phone = "555184472509",
            IsDefault = false,
            IsSelected = false
        }
    ];
}
