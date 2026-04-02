using CommunityToolkit.Mvvm.ComponentModel;
using MuralDigital.Models;

namespace MuralDigital.ViewModels;

public partial class ContactViewModel : ObservableObject
{
    private readonly WhatsAppContact _model;

    public ContactViewModel(WhatsAppContact model)
    {
        _model = model;
        _name = model.Name;
        _phone = model.Phone;
        _isDefault = model.IsDefault;
        _isSelected = model.IsSelected;
    }

    [ObservableProperty]
    private string _name;

    [ObservableProperty]
    private string _phone;

    [ObservableProperty]
    private bool _isDefault;

    [ObservableProperty]
    private bool _isSelected;

    public string DisplayPhone
    {
        get
        {
            var p = Phone;
            if (p.Length >= 12)
                return $"+{p[..2]} {p[2..4]} {p[4..]}";
            return p;
        }
    }

    public WhatsAppContact ToModel() => new()
    {
        Id = _model.Id,
        Name = Name,
        Phone = Phone,
        IsDefault = IsDefault,
        IsSelected = IsSelected
    };
}
