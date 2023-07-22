namespace Freefood;

public partial class MainMenuPage : ContentPage
{
	public MainMenuPage()
	{
		InitializeComponent();
	}

	private void btn_clicked_goToFindFoodPage(object sender, EventArgs e)
	{
		Navigation.PushAsync(new FindFoodPage());
	}

    private void btn_clicked_goToListFoodPage(object sender, EventArgs e)
    {
        Navigation.PushAsync(new ListPage());
    }
}