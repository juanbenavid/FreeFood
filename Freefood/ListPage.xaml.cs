using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Location;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using System.Diagnostics;

namespace Freefood;

public partial class ListPage : ContentPage
{
    private SystemLocationDataSource locationSource = new SystemLocationDataSource();
    private GraphicsOverlay pinOverlay = new GraphicsOverlay();
    private SimpleMarkerSymbol pinSymbol = new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Cross, System.Drawing.Color.Red, 10);
    public ListPage()
    {
        InitializeComponent();
        this.BindingContext = new ListMapViewModel();
        StartLocationServices();   
        mapView.GeoViewTapped += OnMapViewTapped;
        mapView.GraphicsOverlays.Add(pinOverlay);

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

    private async void PinFoodButtonClicked(Object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//FoodFormPage");
    }

    private void NavigationButton_Clicked(object sender, EventArgs e)
    {
        // Starts location display with auto pan mode set to Navigation.
        mapView.LocationDisplay.AutoPanMode = LocationDisplayAutoPanMode.Navigation;

        _ = StartLocationServices();
    }

    private async void OnMapViewTapped(object sender, GeoViewInputEventArgs e)
    {
        var pinPoint = new MapPoint(e.Location.X,e.Location.Y);
        var grapic = new Graphic(pinPoint, pinSymbol);
        pinOverlay.Graphics.Add(grapic);

        bool answer = await DisplayAlert("Pin point?", "Pin a point to this location?", "Yes", "No");
        Debug.WriteLine("Answer: " + answer);
        if (answer)
        {
            PinFoodButtonClicked(sender, e);
        }
        else
        {
            pinOverlay.Graphics.Clear();
        }
    }
}