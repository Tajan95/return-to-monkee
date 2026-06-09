namespace ReturnToMonkee.Features.PersonTest;

public partial class PersonListPage : ContentPage
{
    private readonly IPersonRepository personRepository;

    public PersonListPage(IPersonRepository personRepository)
    {
        InitializeComponent();
        this.personRepository = personRepository;
    }



    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadPersonsAsync();
    }



    private async void OnCreateClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(PersonEditPage));
    }

    private async void OnEditClicked(object sender, EventArgs e)
    {
        if (sender is Button { CommandParameter: Guid id })
        {
            await Shell.Current.GoToAsync($"{nameof(PersonEditPage)}?personId={id}");
        }
    }

    private async void OnDeleteClicked(object sender, EventArgs e)
    {
        if (sender is Button { CommandParameter: Guid id })
        {
            await personRepository.Delete(id);
            await LoadPersonsAsync();
        }
    }



    private async Task LoadPersonsAsync()
    {
        PersonsCollectionView.ItemsSource = await personRepository.GetAll();
    }
}
