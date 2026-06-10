using ReturnToMonkee.Features.BewegungsErinnerungDemo;
using ReturnToMonkee.Features.TestStringDemo;

namespace ReturnToMonkee;

public partial class AppShell : Shell
{
	private readonly IReminderService? reminderService;

	public AppShell(MainPage mainPage, TestStringCrudPage testStringCrudPage, BewegungsErinnerungPage bewegungsErinnerungPage, IReminderService reminderService)
	{
		InitializeComponent();
		HomeShellContent.Content = mainPage;
		TestStringCrudShellContent.Content = testStringCrudPage;
		BewegungsErinnerungShellContent.Content = bewegungsErinnerungPage;
		this.reminderService = reminderService;
		_ = this.reminderService.StartAsync();
	}
}
