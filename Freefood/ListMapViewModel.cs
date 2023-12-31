using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Location;
using Esri.ArcGISRuntime.Mapping;
using Map = Esri.ArcGISRuntime.Mapping.Map;
using Microsoft.Maui.Controls;

namespace Freefood;

/// <summary>
/// Provides map data to an application
/// </summary>
public class ListMapViewModel : INotifyPropertyChanged
{

    public static Uri foodUri = new Uri("https://services8.arcgis.com/LLNIdHmmdjO2qQ5q/arcgis/rest/services/FreeFood/FeatureServer/0");
    public FeatureLayer foodFeatureLayer;

    public ListMapViewModel( )
    {   
        _map = new Map(SpatialReferences.WebMercator)
        {
            InitialViewpoint = new Viewpoint(new Envelope(-180, -85, 180, 85, SpatialReferences.Wgs84)),
            Basemap = new Basemap(BasemapStyle.ArcGISStreets)
        };

        foodFeatureLayer = new FeatureLayer(foodUri);
        _map.OperationalLayers.Add(foodFeatureLayer);

        
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

    /// <summary>
    /// Raises the <see cref="MapViewModel.PropertyChanged" /> event
    /// </summary>
    /// <param name="propertyName">The name of the property that has changed</param>
    protected void OnPropertyChanged([CallerMemberName] string propertyName = null) => 
             PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    public event PropertyChangedEventHandler PropertyChanged;

    public static async void Refresh(Map map)
    {
        map.OperationalLayers.Clear();
        var featureLayer = new FeatureLayer(foodUri);
        map.OperationalLayers.Add(featureLayer);
    }

  
}
