namespace MuralDigital;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();
		Routing.RegisterRoute("preview", typeof(PreviewPage));
	}
}
