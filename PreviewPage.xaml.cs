using MuralDigital.ViewModels;

namespace MuralDigital;

public partial class PreviewPage : ContentPage
{
    public PreviewPage(PreviewViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
