using System.ComponentModel;
using System.Runtime.CompilerServices;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Location;
using Esri.ArcGISRuntime.Mapping;
using Map = Esri.ArcGISRuntime.Mapping.Map;

namespace Freefood
{
    internal class FindFoodViewModel : INotifyPropertyChanged
    {
        public static Uri foodUri = new Uri("https://services8.arcgis.com/LLNIdHmmdjO2qQ5q/arcgis/rest/services/FreeFood/FeatureServer/0");
        private FeatureLayer foodFeatureLayer;

        public FindFoodViewModel()
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

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public event PropertyChangedEventHandler PropertyChanged;

    }
}
