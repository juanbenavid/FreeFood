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
        mapView.Map.Loaded += Map_Loaded;
        mapView.GeoViewTapped += OnMapViewTapped;
        mapView.GraphicsOverlays.Add(pinOverlay);

    }

    private async Task StartLocationServices()
    {
        try
        {
            // Check if location permission granted.
            var status = Microsoft.Maui.ApplicationModel.PermissionStatus.Unknown;
            status = await Microsoft.Maui.ApplicationModel.Permissions.CheckStatusAsync<Microsoft.Maui.ApplicationModel.Permissions.LocationWhenInUse>();

            // Request location permission if not granted.
            if (status != Microsoft.Maui.ApplicationModel.PermissionStatus.Granted)
            {
                status = await Microsoft.Maui.ApplicationModel.Permissions.RequestAsync<Microsoft.Maui.ApplicationModel.Permissions.LocationWhenInUse>();
            }

            // Start the location display once permission is granted.
            if (status == Microsoft.Maui.ApplicationModel.PermissionStatus.Granted)
            {
                await mapView.LocationDisplay.DataSource.StartAsync();
                mapView.LocationDisplay.IsEnabled = true;
            }
            mapView.LocationDisplay.AutoPanMode = LocationDisplayAutoPanMode.Recenter;
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
            await Application.Current.MainPage.DisplayAlert("Couldn't start location", ex.Message, "OK");
        }

        //var status = await Microsoft.Maui.ApplicationModel.Permissions.RequestAsync<Microsoft.Maui.ApplicationModel.Permissions.LocationWhenInUse>();
 
        //mapView.LocationDisplay.DataSource = locationSource;
        //if (status == Microsoft.Maui.ApplicationModel.PermissionStatus.Granted)
        //{
        //    await mapView.LocationDisplay.DataSource.StartAsync();
        //    mapView.LocationDisplay.IsEnabled = true;
        //}

        //mapView.LocationDisplay.AutoPanMode = LocationDisplayAutoPanMode.Recenter;
    }

    private void Map_Loaded(object sender, EventArgs e)
    {
        // Perform actions or set up the map here
        // For example, add layers, set initial viewpoint, etc.
        _ = StartLocationServices();
    }

    private async void FindFoodButtonClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//FindFoodPage");
    }


    private void NavigationButton_Clicked(object sender, EventArgs e)
    {
        // Starts location display with auto pan mode set to Navigation.
        mapView.LocationDisplay.AutoPanMode = LocationDisplayAutoPanMode.Navigation;

        _ = StartLocationServices();
    }

    private async void OnMapViewTapped(object sender, GeoViewInputEventArgs e)
    {
        var pinPoint = new MapPoint(e.Location.X,e.Location.Y, e.Location.SpatialReference);
        var grapic = new Graphic(pinPoint, pinSymbol);
        pinOverlay.Graphics.Add(grapic);

        bool answer = await DisplayAlert("Pin point?", "Pin a point to this location?", "Yes", "No");
        Debug.WriteLine("Answer: " + answer);
        if (answer)
        {
            Navigation.PushAsync(new FoodFormPage(pinPoint));
        }
        else
        {
            pinOverlay.Graphics.Clear();
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        // Unsubscribe from the event
        mapView.Map.Loaded -= Map_Loaded;
    }

}