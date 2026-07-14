namespace CRUD_LOGIN_MAUI;

public partial class SupervisorPage : ContentPage
{
    public SupervisorPage()
    {
        InitializeComponent();
    }

    private async void OnLogoutClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//MainPage");
    }
}
