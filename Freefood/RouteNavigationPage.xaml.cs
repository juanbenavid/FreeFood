using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Location;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using System.Diagnostics;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Navigation;
using Esri.ArcGISRuntime.Tasks.NetworkAnalysis;

using Color = System.Drawing.Color;
using Location = Esri.ArcGISRuntime.Location.Location;
using static System.Formats.Asn1.AsnWriter;

namespace Freefood;

public partial class RouteNavigationPage : ContentPage, IDisposable
{
    private SystemLocationDataSource locationSource = new SystemLocationDataSource();
    private ArcGISFeature feature;
    // Variables for tracking the navigation route.
    private RouteTracker _tracker;
    private RouteResult _routeResult;
    private Route _route;

    // List of driving directions for the route.
    private IReadOnlyList<DirectionManeuver> _directionsList;

    // Cancellation token for speech synthesizer.
    private CancellationTokenSource _speechToken = new CancellationTokenSource();

    // Graphics to show progress along the route.
    private Graphic _routeAheadGraphic;
    private Graphic _routeTraveledGraphic;

    private readonly Uri _routingUri = new Uri("https://gis.cdph.ca.gov/gisadmin/rest/services/Routing/NetworkAnalysis/NAServer/Route");

    private double dest_x;
    private double dest_y;
    public RouteNavigationPage(double X, double Y)
    {
        InitializeComponent();
        this.dest_x = X;
        this.dest_y = Y;
        this.BindingContext = new RouteNavigationViewModel();
        NavigationMapView.Map.Loaded += Map_Loaded;
        _ = Initialize();
    }

    private async Task Initialize()
    {
        try
        {
            // Create the route task, using the online routing service.
            RouteTask routeTask = await RouteTask.CreateAsync(_routingUri);

            // Get the default route parameters.
            RouteParameters routeParams = await routeTask.CreateDefaultParametersAsync();

            // Explicitly set values for parameters.
            routeParams.ReturnDirections = true;
            routeParams.ReturnStops = true;
            routeParams.ReturnRoutes = true;
            routeParams.OutputSpatialReference = SpatialReferences.WebMercator;

            GeolocationRequest request = new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(10));

            CancellationTokenSource _cancelTokenSource = new CancellationTokenSource();

            Microsoft.Maui.Devices.Sensors.Location location = await Geolocation.Default.GetLocationAsync(request, _cancelTokenSource.Token);


            // Create stops for each location.
            MapPoint _origin = new MapPoint(location.Longitude, location.Latitude, SpatialReferences.Wgs84);
            MapPoint _destination = new MapPoint(dest_x, dest_y, SpatialReferences.WebMercator);

            // Create stops for each location.
            Stop stop1 = new Stop(_origin) { Name = "origin" };
            Stop stop2 = new Stop(_destination) { Name = "destination" };

            // Assign the stops to the route parameters.
            List<Stop> stopPoints = new List<Stop> { stop1, stop2 };
            routeParams.SetStops(stopPoints);

            // Get the route results.
            _routeResult = await routeTask.SolveRouteAsync(routeParams);
            _route = _routeResult.Routes[0];

            // Add a graphics overlay for the route graphics.
            NavigationMapView.GraphicsOverlays.Add(new GraphicsOverlay());

            // Add graphics for the stops.
            SimpleMarkerSymbol stopSymbol = new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Diamond, Color.Red, 30);
            NavigationMapView.GraphicsOverlays[0].Graphics.Add(new Graphic(_destination, stopSymbol));

            SimpleMarkerSymbol startSymbol = new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Diamond, Color.Green, 30);
            NavigationMapView.GraphicsOverlays[0].Graphics.Add(new Graphic(_origin, startSymbol));

            // Create a graphic (with a dashed line symbol) to represent the route.
            _routeAheadGraphic = new Graphic(_route.RouteGeometry) { Symbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Dash, Color.BlueViolet, 5) };

            // Create a graphic (solid) to represent the route that's been traveled (initially empty).
            _routeTraveledGraphic = new Graphic { Symbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, Color.LightBlue, 3) };

            // Add the route graphics to the map view.
            NavigationMapView.GraphicsOverlays[0].Graphics.Add(_routeAheadGraphic);
            NavigationMapView.GraphicsOverlays[0].Graphics.Add(_routeTraveledGraphic);

            // Set the map viewpoint to show the entire route.
            await NavigationMapView.SetViewpointGeometryAsync(_route.RouteGeometry, 100);

            activityIndicator.IsRunning = false;

            // Enable the navigation button.
            //StartNavigationButton.IsEnabled = true;

            //StartNavigation();

        }
        catch (Exception e)
        {
            activityIndicator.IsRunning = false;
            await Application.Current.MainPage.DisplayAlert("Error", e.Message, "OK");

        }
    }

    private void StartNavigation()
    {
        // Get the directions for the route.
        _directionsList = _route.DirectionManeuvers;

        // Create a route tracker.
        _tracker = new RouteTracker(_routeResult, 0, true);
        _tracker.NewVoiceGuidance += SpeakDirection;

        // Handle route tracking status changes.
        _tracker.TrackingStatusChanged += TrackingStatusUpdated;

        // Turn on navigation mode for the map view.
        NavigationMapView.LocationDisplay.AutoPanMode = LocationDisplayAutoPanMode.Navigation;
        //NavigationMapView.LocationDisplay.AutoPanModeChanged += AutoPanModeChanged;

        // Use this instead if you want real location:
        NavigationMapView.LocationDisplay.DataSource = new RouteTrackerLocationDataSource( _tracker, new SystemLocationDataSource());

        // Enable the location display (this wil start the location data source).
        NavigationMapView.LocationDisplay.IsEnabled = true;

        // Show the message block for text output.
        MessagesTextBlock.IsVisible = true;
    }

    private void TrackingStatusUpdated(object sender, RouteTrackerTrackingStatusChangedEventArgs e)
    {
        TrackingStatus status = e.TrackingStatus;

        // Start building a status message for the UI.
        System.Text.StringBuilder statusMessageBuilder = new System.Text.StringBuilder();

        // Check the destination status.
        if (status.DestinationStatus == DestinationStatus.NotReached || status.DestinationStatus == DestinationStatus.Approaching)
        {
            statusMessageBuilder.AppendLine("Distance remaining: " +
                                        status.RouteProgress.RemainingDistance.DisplayText + " " +
                                        status.RouteProgress.RemainingDistance.DisplayTextUnits.PluralDisplayName);

            statusMessageBuilder.AppendLine("Time remaining: " +
                                            status.RouteProgress.RemainingTime.ToString(@"hh\:mm\:ss"));

            if (status.CurrentManeuverIndex + 1 < _directionsList.Count)
            {
                statusMessageBuilder.AppendLine("Next direction: " + _directionsList[status.CurrentManeuverIndex + 1].DirectionText);
            }

            // Set geometries for progress and the remaining route.
            _routeAheadGraphic.Geometry = status.RouteProgress.RemainingGeometry;
            _routeTraveledGraphic.Geometry = status.RouteProgress.TraversedGeometry;
        }
        else if (status.DestinationStatus == DestinationStatus.Reached)
        {
            statusMessageBuilder.AppendLine("Destination reached.");

            // Set the route geometries to reflect the completed route.
            _routeAheadGraphic.Geometry = null;
            _routeTraveledGraphic.Geometry = status.RouteResult.Routes[0].RouteGeometry;

            // Navigate to the next stop (if there are stops remaining).
            if (status.RemainingDestinationCount > 1)
            {
                _tracker.SwitchToNextDestinationAsync();
            }

            // Stop the simulated location data source.
            NavigationMapView.LocationDisplay.DataSource.StopAsync();
        }

        Microsoft.Maui.ApplicationModel.MainThread.BeginInvokeOnMainThread(() =>
        {
            // Show the status information in the UI.
            MessagesTextBlock.Text = statusMessageBuilder.ToString().TrimEnd('\n').TrimEnd('\r');
        });
    }

    private void SpeakDirection(object sender, RouteTrackerNewVoiceGuidanceEventArgs e)
    {
        // Say the direction using voice synthesis.
        if (e.VoiceGuidance.Text?.Length > 0)
        {
            _speechToken.Cancel();
            _speechToken = new CancellationTokenSource();
            TextToSpeech.Default.SpeakAsync(e.VoiceGuidance.Text, null, _speechToken.Token);
        }
    }

    //private void AutoPanModeChanged(object sender, LocationDisplayAutoPanMode e)
    //{
    //    // Turn the recenter button on or off when the location display changes to or from navigation mode.
    //    RecenterButton.IsEnabled = e != LocationDisplayAutoPanMode.Navigation;
    //}


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
                await NavigationMapView.LocationDisplay.DataSource.StartAsync();
                NavigationMapView.LocationDisplay.IsEnabled = true;
            }
            NavigationMapView.LocationDisplay.AutoPanMode = LocationDisplayAutoPanMode.Recenter;
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
            await Application.Current.MainPage.DisplayAlert("Couldn't start location", ex.Message, "OK");
        }
    }

    private void Map_Loaded(object sender, EventArgs e)
    {
        // Perform actions or set up the map here
        // For example, add layers, set initial viewpoint, etc.
        _ = StartLocationServices();
    }

    private void NavigationButton_Clicked(object sender, EventArgs e)
    {
        // Starts location display with auto pan mode set to Navigation.
        NavigationMapView.LocationDisplay.AutoPanMode = LocationDisplayAutoPanMode.Navigation;

        _ = StartLocationServices();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        NavigationMapView.Map.Loaded -= Map_Loaded;
    }

    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);

        if (NavigationMapView.Map != null) { ListMapViewModel.Refresh(NavigationMapView.Map); NavigationMapView.GraphicsOverlays.First().Graphics.Clear(); }

    }

    public void Dispose()
    {
        // Stop the tracker.
        if (_tracker != null)
        {
            _tracker.TrackingStatusChanged -= TrackingStatusUpdated;
            _tracker.NewVoiceGuidance -= SpeakDirection;
            _tracker = null;
        }

        // Stop the location data source.
        NavigationMapView.LocationDisplay?.DataSource?.StopAsync();
    }
}