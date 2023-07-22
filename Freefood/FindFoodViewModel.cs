using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Location;
using Esri.ArcGISRuntime.Mapping;
using Map = Esri.ArcGISRuntime.Mapping.Map;
using Microsoft.Maui.Controls;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;


namespace Freefood
{
    internal class FindFoodViewModel : INotifyPropertyChanged
    {
        public static Uri foodUri = new Uri("https://services8.arcgis.com/LLNIdHmmdjO2qQ5q/arcgis/rest/services/FreeFood/FeatureServer/0");
        private FeatureLayer foodFeatureLayer;
        private ContentPage myPage;
        private MapView mapView;

        public FindFoodViewModel(ContentPage myPage)
        {
            _map = new Map(SpatialReferences.WebMercator)
            {
                InitialViewpoint = new Viewpoint(new Envelope(-180, -85, 180, 85, SpatialReferences.Wgs84)),
                Basemap = new Basemap(BasemapStyle.ArcGISStreets)
            };

            foodFeatureLayer = new FeatureLayer(foodUri);
            _map.OperationalLayers.Add(foodFeatureLayer);
            this.myPage = myPage;
            this.mapView = mapView;
        }

        public async Task QueryFeatureLayer(string layerId, string whereExpression, Envelope queryExtent)
        {
            // Get the layer based on its Id.
            var featureLayerToQuery = _map?.OperationalLayers[layerId] as FeatureLayer;

            // Get the feature table from the feature layer.
            var featureTableToQuery = featureLayerToQuery?.FeatureTable;
            if (featureTableToQuery == null) { return; }
            // Clear any existing selection.
            featureLayerToQuery?.ClearSelection();

            // Create the query parameters using the where expression and extent passed in.
            QueryParameters queryParams = new QueryParameters
            {
                Geometry = queryExtent,
                ReturnGeometry = true,
                WhereClause = "Quantity >= 5",
            };

            // Query the table and get the list of features in the result.
            var queryResult = await featureTableToQuery.QueryFeaturesAsync(queryParams);

            // Loop over each feature from the query result.
            foreach (Feature feature in queryResult)
            {
                // Select each feature.
                featureLayerToQuery!.SelectFeature(feature);
            }
        }

        private Esri.ArcGISRuntime.Mapping.Map _map;

        /// <summary>
        /// Gets or sets the map
        /// </summary>
        public Esri.ArcGISRuntime.Mapping.Map FoodMap
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

    }

}
