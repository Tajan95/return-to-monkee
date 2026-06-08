using ReturnToMonkee.Features.TestStringDemo;

namespace ReturnToMonkee;

public partial class AppShell : Shell
{
	public AppShell(MainPage mainPage, TestStringCrudPage testStringCrudPage)
	{
		InitializeComponent();
		HomeShellContent.Content = mainPage;
		TestStringCrudShellContent.Content = testStringCrudPage;
	}
}
