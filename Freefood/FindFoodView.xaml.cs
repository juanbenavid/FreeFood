using Esri.ArcGISRuntime.Location;
using Esri.ArcGISRuntime.UI;

namespace Freefood;

public partial class FindFoodPage : ContentPage
{
    private SystemLocationDataSource locationSource = new SystemLocationDataSource();
    public FindFoodPage()
	{
		InitializeComponent();
        this.BindingContext = new FindFoodViewModel();
        StartLocationServices();
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

}

