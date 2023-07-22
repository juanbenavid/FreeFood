namespace Freefood;

public partial class FoodFormPage : ContentPage
{
	public FoodFormPage()
	{
		InitializeComponent();
	}

    public async void BackHome(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//MainPage");
    }
}