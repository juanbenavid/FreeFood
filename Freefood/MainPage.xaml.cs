using Esri.ArcGISRuntime.Location;
using Esri.ArcGISRuntime.UI;

namespace Freefood;

public partial class MainPage : ContentPage
{
    private SystemLocationDataSource locationSource = new SystemLocationDataSource();
    public MainPage()
    {
        InitializeComponent();
        this.BindingContext = new MapViewModel();
        StartLocation();   

    }

    private async Task StartLocation()
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

    private async void PinFoodButtonClicked(Object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//FoodFormPage");
    }
}

