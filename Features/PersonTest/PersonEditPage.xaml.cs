namespace ReturnToMonkee.Features.PersonTest;

[QueryProperty(nameof(PersonId), "personId")]
public partial class PersonEditPage : ContentPage
{
    private readonly IPersonRepository personRepository;
    private Guid? currentPersonId;

    public PersonEditPage(IPersonRepository personRepository)
    {
        InitializeComponent();
        this.personRepository = personRepository;
    }



    public string? PersonId
    {
        set
        {
            if (Guid.TryParse(value, out var id))
            {
                currentPersonId = id;
                _ = LoadPersonAsync(id);
            }
        }
    }



    private async Task LoadPersonAsync(Guid id)
    {
        var person = await personRepository.Get(id);

        if (person is null)
        {
            return;
        }

        FirstNameEntry.Text = person.FirstName;
        LastNameEntry.Text = person.LastName;
    }



    private async void OnSaveClicked(object sender, EventArgs e)
    {
        var firstName = FirstNameEntry.Text?.Trim() ?? string.Empty;
        var lastName = LastNameEntry.Text?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
        {
            ValidationLabel.Text = "Vorname und Nachname dürfen nicht leer sein.";
            ValidationLabel.IsVisible = true;
            return;
        }

        // Wenn Id vorhanden, dann update
        if (currentPersonId is Guid id)
        {
            await personRepository.Update(new Person
            {
                Id = id,
                FirstName = firstName,
                LastName = lastName,
            });
        }
        else
        {
            await personRepository.Create(new Person
            {
                FirstName = firstName,
                LastName = lastName,
            });
        }

        await Shell.Current.GoToAsync("..");
    }



    private async void OnCancelClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
}
