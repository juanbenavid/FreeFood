namespace Freefood;

public partial class MainMenuPage : ContentPage
{
	public MainMenuPage()
	{
		InitializeComponent();
        NavigationPage.SetHasNavigationBar(this, false);
        NavigationPage.SetBackButtonTitle(this, null);
        //NavigationPage.SetTitleIconImageSource(this, "app_title_icon.png");
    }

	private void btn_clicked_goToFindFoodPage(object sender, EventArgs e)
	{
		Navigation.PushAsync(new ListPage());
	}

    private void btn_clicked_goToListFoodPage(object sender, EventArgs e)
    {
        Navigation.PushAsync(new ListPage());
    }

    private void btn_clicked_goToDonationFoodPage(object sender, EventArgs e)
    {
        Navigation.PushAsync(new DonationPage());
    }
}