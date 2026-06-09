using ReturnToMonkee.Features.PersonTest;

namespace ReturnToMonkee;

public partial class AppShell : Shell
{
	public AppShell(MainPage mainPage, PersonListPage personListPage)
	{
		InitializeComponent();
		Routing.RegisterRoute(nameof(PersonEditPage), typeof(PersonEditPage));
		HomeShellContent.Content = mainPage;
		PersonListShellContent.Content = personListPage;
	}
}
