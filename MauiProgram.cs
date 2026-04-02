using Microsoft.Extensions.Logging;
using MuralDigital.Services;
using MuralDigital.ViewModels;

namespace MuralDigital;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

		// Services
		builder.Services.AddSingleton<HttpClient>();
		builder.Services.AddSingleton<IUrlShortenerService, TinyUrlService>();
		builder.Services.AddSingleton<IMuralDataService, MuralDataService>();
		builder.Services.AddSingleton<IWhatsAppTextGenerator, WhatsAppTextGenerator>();

		// ViewModels
		builder.Services.AddSingleton<MainViewModel>();
		builder.Services.AddTransient<PreviewViewModel>();

		// Pages
		builder.Services.AddSingleton<MainPage>();
		builder.Services.AddTransient<PreviewPage>();

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
