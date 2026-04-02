using MuralDigital.ViewModels;

namespace MuralDigital;

public partial class MainPage : ContentPage
{
	private bool _dataLoaded;

	public MainPage(MainViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}

	protected override async void OnAppearing()
	{
		base.OnAppearing();
		if (!_dataLoaded && BindingContext is MainViewModel vm)
		{
			await vm.LoadDataCommand.ExecuteAsync(null);
			_dataLoaded = true;
		}
	}
}
