using Microsoft.Extensions.DependencyInjection;

namespace ReturnToMonkee;

public partial class App : Microsoft.Maui.Controls.Application
{
	private readonly IServiceProvider services;

	public App(IServiceProvider services)
	{
		// InitializeComponent laedt zuerst die App-weiten MergedDictionaries (Colors/Icons/Styles).
		// Erst danach (in CreateWindow) wird die AppShell inkl. eager injizierter Seiten aufgeloest,
		// sodass deren StaticResources bereits verfuegbar sind.
		InitializeComponent();
		this.services = services;
	}

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var shell = services.GetRequiredService<AppShell>();
        return new Window(shell);
    }
}
