using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geotriggers;
using Esri.ArcGISRuntime.Location;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime;

namespace Freefood;

public partial class FindFoodPage : ContentPage
{
    private SystemLocationDataSource locationSource = new SystemLocationDataSource();

    //Geotriggers
    private LocationGeotriggerFeed _geotriggerFeed;
    private ServiceFeatureTable foodPoints;
    private GeotriggerMonitor _foodMonitor;
    private List<GeotriggerFeature> _features = new List<GeotriggerFeature>();

    public FindFoodPage()
    {
        InitializeComponent();
        this.BindingContext = new FindFoodViewModel(this);
        _ = StartLocationServices();
        _ = Initialize();
    }

    private async Task Initialize()
    {
        foodPoints = ((FeatureLayer)mapView.Map.OperationalLayers[0]).FeatureTable as ServiceFeatureTable;
        await locationSource?.StartAsync();

        _geotriggerFeed = new LocationGeotriggerFeed(locationSource);

        _foodMonitor = CreateGeotriggerMonitor(foodPoints, 3.0, "Food Geotrigger");

        await _foodMonitor?.StartAsync();
    }

    private GeotriggerMonitor CreateGeotriggerMonitor(ServiceFeatureTable table, double bufferSize, string triggerName)
    {
        // Create parameters for the fence.
        FeatureFenceParameters fenceParameters = new FeatureFenceParameters(table, bufferSize);

        // The ArcadeExpression defined in the following FenceGeotrigger returns the value for the "name" field of the feature that triggered the monitor.
        FenceGeotrigger fenceTrigger = new FenceGeotrigger(_geotriggerFeed, FenceRuleType.EnterOrExit, fenceParameters, new ArcadeExpression("$fenceFeature.name"), triggerName);

        // Create the monitor and set its event handler for notifications.
        GeotriggerMonitor geotriggerMonitor = new GeotriggerMonitor(fenceTrigger);
        geotriggerMonitor.Notification += HandleGeotriggerNotification;

        return geotriggerMonitor;
    }

    private void HandleGeotriggerNotification(object sender, GeotriggerNotificationInfo info)
    {
        Application.Current.MainPage.DisplayAlert("Entered Region", "Around some food.", "OK");
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


    private void NavigationButton_Clicked(object sender, EventArgs e)
    {
        // Starts location display with auto pan mode set to Navigation.
        mapView.LocationDisplay.AutoPanMode = LocationDisplayAutoPanMode.Navigation;

        _ = StartLocationServices();
    }

}

public class GeotriggerFeature
{
    public string Name { get; set; }

    public string Description { get; set; }

    public ImageSource ImageSource { get; set; }
}
