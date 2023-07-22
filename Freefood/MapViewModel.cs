using System.ComponentModel;
using System.Runtime.CompilerServices;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Geotriggers;
using Esri.ArcGISRuntime.Location;
using Esri.ArcGISRuntime.Mapping;
using Map = Esri.ArcGISRuntime.Mapping.Map;

namespace Freefood;

/// <summary>
/// Provides map data to an application
/// </summary>
public class MapViewModel : INotifyPropertyChanged
{

    private static Uri foodUri = new Uri("https://services8.arcgis.com/LLNIdHmmdjO2qQ5q/arcgis/rest/services/FreeFood/FeatureServer/0");
    private FeatureLayer foodFeatureLayer;

    private LocationGeotriggerFeed _geotriggerFeed;
    private GeotriggerMonitor _pointsMonitor;

    private ServiceFeatureTable _foodPoints;

    public MapViewModel()
    {   
        _map = new Map(SpatialReferences.WebMercator)
        {
            InitialViewpoint = new Viewpoint(new Envelope(-180, -85, 180, 85, SpatialReferences.Wgs84)),
            Basemap = new Basemap(BasemapStyle.ArcGISStreets)
        };

        foodFeatureLayer = new FeatureLayer(foodUri);
        _map.OperationalLayers.Add(foodFeatureLayer);

        _pointsMonitor = CreateGeotriggerMonitor(_foodPoints, 3.0, "POI Geotrigger");
    }

    public void zoomToUser(MapPoint location)
    {
        return;
    }


    private Esri.ArcGISRuntime.Mapping.Map _map;

    /// <summary>
    /// Gets or sets the map
    /// </summary>
    public Esri.ArcGISRuntime.Mapping.Map Map
    {
        get => _map;
        set { _map = value; OnPropertyChanged(); }
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
        Application.Current.MainPage.DisplayAlert("Alert", "Alert", "OK");
    }

        /// <summary>
        /// Raises the <see cref="MapViewModel.PropertyChanged" /> event
        /// </summary>
        /// <param name="propertyName">The name of the property that has changed</param>
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null) => 
             PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    public event PropertyChangedEventHandler PropertyChanged;
}
