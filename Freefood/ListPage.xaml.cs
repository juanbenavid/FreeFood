﻿using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Geotriggers;
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

    private FeatureTable foodFeatureTable;
    ServiceGeodatabase serviceGeodatabase = new ServiceGeodatabase(ListMapViewModel.foodUri);
    GeotriggerMonitor _geotriggerMonitor;
    private LocationGeotriggerFeed _locationFeed;

    public ListPage()
    {
        InitializeComponent();
        this.BindingContext = new ListMapViewModel();
        mapView.GeoViewTapped += OnMapViewTapped;
        mapView.GraphicsOverlays.Add(pinOverlay);

        _ = StartLocationServices();
        InitializeGeotrigger();

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

    private async Task InitializeGeotrigger()
    {
        await LoadFeatureTable();
        _locationFeed = new LocationGeotriggerFeed(locationSource);
        var fenceParameters = new FeatureFenceParameters(foodFeatureTable, 25);
        var fenceGeotrigger = new FenceGeotrigger(_locationFeed, FenceRuleType.Enter, fenceParameters);
        // Create a GeotriggerMonitor to monitor the FenceGeotrigger created previously.
        _geotriggerMonitor = new GeotriggerMonitor(fenceGeotrigger);

        // Handle notification events (when a fence is entered).
        _geotriggerMonitor.Notification += NotifyFoodAreaEntered;

        // Start monitoring.
        await _geotriggerMonitor.StartAsync();
    }

    private void NotifyFoodAreaEntered(object sender, GeotriggerNotificationInfo e)
    {
        GeoElement fence = (e as FenceGeotriggerNotificationInfo).FenceGeoElement;
        Dispatcher.Dispatch(async () => await DisplayTriggerAlert(fence));

    }

    private async Task DisplayTriggerAlert(GeoElement fence)
    {
        string action = await DisplayActionSheet(fence.Attributes["Title"].ToString() + " Near you!", "Back", null, "More details", "I'm here and the food is gone 😢");
        Debug.WriteLine("Action: " + action);
       if (action == "More details")
        {
            Navigation.PushAsync(new FeaturePage((ArcGISFeature)fence,(fence.Geometry as MapPoint).X, (fence.Geometry as MapPoint).Y));
        }
    }
 
    public async Task LoadFeatureTable()
    {
        await serviceGeodatabase.LoadAsync();
        foodFeatureTable = serviceGeodatabase.GetTable(0);
        await foodFeatureTable.LoadAsync();
    }

    private void Map_Loaded(object sender, EventArgs e)
    {
        // Perform actions or set up the map here
        // For example, add layers, set initial viewpoint, etc.
        _ = StartLocationServices();
        _ = InitializeGeotrigger();
    }

    private void NavigationButton_Clicked(object sender, EventArgs e)
    {
        // Starts location display with auto pan mode set to Navigation.
        mapView.LocationDisplay.AutoPanMode = LocationDisplayAutoPanMode.Navigation;

        _ = StartLocationServices();
    }

    private async void OnMapViewTapped(object sender, GeoViewInputEventArgs e)
    {
        /// if feature tapped, give info. 
        /// else ask to create a new feature.
        var layer = mapView.Map.OperationalLayers[0];
        IdentifyLayerResult identifyResult = await mapView.IdentifyLayerAsync(layer, e.Position, 2, false);
        if (identifyResult.GeoElements.Any())
        {
            GeoElement tappedElement = identifyResult.GeoElements.First();
            ArcGISFeature tappedFeature = (ArcGISFeature)tappedElement;
            await tappedFeature.LoadAsync();

            bool moreInfo = await DisplayAlert(
                tappedFeature.Attributes["Title"]?.ToString() ?? "No Title", 
                tappedFeature.Attributes["Description"]?.ToString() ?? "No Description",
                "See full info", "Back");
            if (moreInfo)
            {
                Navigation.PushAsync(new FeaturePage(tappedFeature, e.Location.X, e.Location.Y));
            }
            return;
        }

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
        mapView.Map.Loaded -= Map_Loaded;
    }

    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);

        if (mapView.Map != null) { ListMapViewModel.Refresh(mapView.Map); mapView.GraphicsOverlays.First().Graphics.Clear(); }
        
    }


}