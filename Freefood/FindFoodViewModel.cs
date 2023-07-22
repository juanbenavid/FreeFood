using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Location;
using Esri.ArcGISRuntime.Mapping;
using Map = Esri.ArcGISRuntime.Mapping.Map;
using Microsoft.Maui.Controls;

namespace Freefood
{
    internal class FindFoodViewModel : INotifyPropertyChanged
    {
        public static Uri foodUri = new Uri("https://services8.arcgis.com/LLNIdHmmdjO2qQ5q/arcgis/rest/services/FreeFood/FeatureServer/0");
        private FeatureLayer foodFeatureLayer;
        private FeatureLayer displayFeatureLayer;
        private ContentPage myPage;
        private MapView mapView;
        FeatureTable featuresToDisplay;

        public FindFoodViewModel(ContentPage myPage)
        {
            _map = new Map(SpatialReferences.WebMercator)
            {
                InitialViewpoint = new Viewpoint(new Envelope(-180, -85, 180, 85, SpatialReferences.Wgs84)),
                Basemap = new Basemap(BasemapStyle.ArcGISStreets)
            };

            foodFeatureLayer = new FeatureLayer(foodUri);
            displayFeatureLayer = foodFeatureLayer;
            _map.OperationalLayers.Add(displayFeatureLayer);
            this.myPage = myPage;
            this.mapView = mapView;
        }


        public async Task SetAllHidden()
        {
            FeatureQueryResult allFeatures = await featuresToDisplay.QueryFeaturesAsync(null);
            foreach (var feature in allFeatures)
            {
                for (int i = 0; i < allFeatures.Count<Feature>(); i++)
                {
                    displayFeatureLayer.SetFeatureVisible(allFeatures.ElementAt<Feature>(i), false);
                }
            }
        }

        public async Task UpdateFeaturesToBeDisplayed(List<Feature> visibleFeatures, List<Feature> hiddenFeatures)
        {
            //Set all the visible features
            displayFeatureLayer.ClearSelection();
            displayFeatureLayer.SelectFeatures(visibleFeatures);
            FeatureQueryResult result = await displayFeatureLayer.GetSelectedFeaturesAsync();
            for(int i = 0; i < result.Count<Feature>(); i++)
            {
                displayFeatureLayer.SetFeatureVisible(result.ElementAt<Feature>(i), true);
            }

            //Set all the hidden features
            displayFeatureLayer.ClearSelection();
            displayFeatureLayer.SelectFeatures(hiddenFeatures);
            result = await displayFeatureLayer.GetSelectedFeaturesAsync();
            for(int i = 0; i < result.Count<Feature>(); i++)
            {
                displayFeatureLayer.SetFeatureVisible(result.ElementAt<Feature>(i), false);
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
        /// Raises the <see cref="FindFoodViewModel.PropertyChanged" /> event
        /// </summary>
        /// <param name="propertyName">The name of the property that has changed</param>
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public event PropertyChangedEventHandler PropertyChanged;

    }

}
