using Esri.ArcGISRuntime.Location;
using Esri.ArcGISRuntime.UI;
using System.Diagnostics;

namespace Freefood;

public partial class ListPage : ContentPage
{
    private SystemLocationDataSource locationSource = new SystemLocationDataSource();
    public ListPage()
    {
        InitializeComponent();
        this.BindingContext = new ListMapViewModel(this);
        _ = StartLocationServices();   
        mapView.GeoViewTapped += OnMapViewTapped;

    }

    private async Task StartLocationServices()
    {
        var status = await Microsoft.Maui.ApplicationModel.Permissions.RequestAsync<Microsoft.Maui.ApplicationModel.Permissions.LocationWhenInUse>();

        mapView.LocationDisplay.DataSource = locationSource;
        if (status == Microsoft.Maui.ApplicationModel.PermissionStatus.Granted)
        {
            await mapView.LocationDisplay.DataSource.StartAsync();
            mapView.LocationDisplay.IsEnabled = true;
        }

        mapView.LocationDisplay.AutoPanMode = LocationDisplayAutoPanMode.Recenter;

    }

    private void FindFoodButtonClicked(object sender, EventArgs e)
    {

    }

    private void PinFoodButtonClicked(Object sender, EventArgs e)
    {
        Navigation.PushAsync(new FoodFormPage());
        //await Shell.Current.GoToAsync("//FoodFormPage");
    }

    private void NavigationButton_Clicked(object sender, EventArgs e)
    {
        // Starts location display with auto pan mode set to Navigation.
        mapView.LocationDisplay.AutoPanMode = LocationDisplayAutoPanMode.Navigation;

        _ = StartLocationServices();
    }

    private async void OnMapViewTapped(object sender, GeoViewInputEventArgs e)
    {
        bool answer = await DisplayAlert("Pin point?", "Pin a point to this location:" + e.Location.ToString(), "Yes", "No");
        Debug.WriteLine("Answer: " + answer);
        if (answer)
        {
            PinFoodButtonClicked(sender, e);
        }
    }
}