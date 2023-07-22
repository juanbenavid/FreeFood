﻿using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Map = Esri.ArcGISRuntime.Mapping.Map;


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
