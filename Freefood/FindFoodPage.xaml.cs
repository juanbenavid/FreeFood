using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geotriggers;
using Esri.ArcGISRuntime.Location;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime;
using System.ComponentModel;
using Esri.ArcGISRuntime.Symbology;
using System.Diagnostics;

namespace Freefood;

public partial class FindFoodPage : ContentPage, INotifyPropertyChanged
{
    private SystemLocationDataSource locationSource = new SystemLocationDataSource();

    //Geotriggers
    private LocationGeotriggerFeed _locationFeed;
    private ServiceFeatureTable foodPoints;
    private GeotriggerMonitor _foodMonitor;
    internal List<Feature> _visibleFeatures = new List<Feature>();
    internal List<Feature> _hiddenFeatures = new List<Feature>();
    internal FindFoodViewModel vm;
    internal FeatureTable featuresToDisplay;


    public FindFoodPage()
    {
        InitializeComponent();
        vm = new FindFoodViewModel(this);
        this.BindingContext = vm;
        this.featuresToDisplay = vm.featuresToDisplay;
        _ = Initialize();
        //_ = vm.SetAllHidden();
        mapView.Map.Loaded += Map_Loaded;
    }

    private async Task Initialize()
    {
        foodPoints = ((FeatureLayer)mapView.Map.OperationalLayers[0]).FeatureTable as ServiceFeatureTable;
        await locationSource?.StartAsync();

        //FeatureQueryResult allFeatures = await featuresToDisplay.QueryFeaturesAsync(null);
        //foreach (var feature in allFeatures)
        //{
        //    for (int i = 0; i < allFeatures.Count<Feature>(); i++)
        //    {
        //        _hiddenFeatures.Add(allFeatures.ElementAt<Feature>(i));
        //    }
        //}

        //_locationFeed = new LocationGeotriggerFeed(locationSource);
        //_foodMonitor = CreateGeotriggerMonitor(foodPoints, 3000, "Food Geotrigger");
        //await _foodMonitor?.StartAsync();


    }

    private GeotriggerMonitor CreateGeotriggerMonitor(ServiceFeatureTable table, double bufferSize, string triggerName)
    {
        // Create parameters for the fence.
        FeatureFenceParameters fenceParameters = new FeatureFenceParameters(table, bufferSize);

        // The ArcadeExpression defined in the following FenceGeotrigger returns the value for the "name" field of the feature that triggered the monitor.
        FenceGeotrigger fenceTrigger = new FenceGeotrigger(_locationFeed, FenceRuleType.EnterOrExit, fenceParameters);

        // Create the monitor and set its event handler for notifications.
        GeotriggerMonitor geotriggerMonitor = new GeotriggerMonitor(fenceTrigger);
        geotriggerMonitor.Notification += HandleGeotriggerNotification;

        return geotriggerMonitor;
    }

    private void HandleGeotriggerNotification(object sender, GeotriggerNotificationInfo e)
    {
        GeoElement fence = (e as FenceGeotriggerNotificationInfo).FenceGeoElement;

        string title = fence.Attributes["Title"]?.ToString();
        //string startDate = fence.Attributes["StartDate"]?.ToString();
        //string endDate = fence.Attributes["EndDate"]?.ToString();
        //string description = fence.Attributes["Description"]?.ToString();
        //string categories = fence.Attributes["Categories"]?.ToString();
        //string navigationDetails = fence.Attributes["NavigationDetails"]?.ToString();
        //string donation = fence.Attributes["Donation"]?.ToString();
            //int there = int.Parse(fence.Attributes["There"]?.ToString());
            //int gone = int.Parse(fence.Attributes["Gone"]?.ToString());

            //if ((there + gone) > 25 && gone > there)
            //    return;
            if (e is FenceGeotriggerNotificationInfo fenceInfo)
            {
                if (fenceInfo.FenceNotificationType == FenceNotificationType.Entered)
                {
                    _visibleFeatures.Add((Feature)fence);
                    if(_hiddenFeatures.Contains((Feature)fence))
                    {
                        _hiddenFeatures.Remove((Feature)fence);
                    }
                }
                else if (fenceInfo.FenceNotificationType == FenceNotificationType.Exited)
                {
                    _hiddenFeatures.Add((Feature)fence);
                    if(_visibleFeatures.Contains((Feature)fence))
                    {
                        _visibleFeatures.Remove((Feature)fence);
                    }
                }
            }

            _ = vm.UpdateFeaturesToBeDisplayed(_visibleFeatures, _hiddenFeatures);
        //string dietaryInformation = fence.Attributes["DietaryInfo"]?.ToString();

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


    private void NavigationButton_Clicked(object sender, EventArgs e)
    {
        // Starts location display with auto pan mode set to Navigation.
        mapView.LocationDisplay.AutoPanMode = LocationDisplayAutoPanMode.Navigation;

        _ = StartLocationServices();
    }

    private void Map_Loaded(object sender, EventArgs e)
    {
        // Perform actions or set up the map here
        // For example, add layers, set initial viewpoint, etc.
        _ = StartLocationServices();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        mapView.Map.Loaded -= Map_Loaded;
    }

    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);

        if (mapView.Map != null) { ListMapViewModel.Refresh(mapView.Map); mapView.GraphicsOverlays.First().Graphics.Clear(); }

    }

}

public class GeotriggerFeature
{
    public string Name { get; set; }

    public string Description { get; set; }

    public ImageSource ImageSource { get; set; }
}
