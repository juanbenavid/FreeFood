using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using System.Formats.Asn1;

namespace Freefood;

public partial class FoodFormPage : ContentPage
{
    public List<string> FoodCategories = new List<string>
    { "Pizza", "Beverages", "Candy", "Catered Food", "Chips", "Desserts", "Fruit", "Other", "Salad", "Sandwiches", "Snacks" };


    private FeatureTable foodFeatureTable;
    ServiceGeodatabase serviceGeodatabase = new ServiceGeodatabase(ListMapViewModel.foodUri);
    private MapPoint pinPoint;

    public FoodFormPage(MapPoint pinPoint)
	{
		InitializeComponent();
        CategoryPicker.ItemsSource = FoodCategories;
        LoadFeatureTable();
        this.pinPoint = pinPoint;
    }

    //public async void BackHome(object sender, EventArgs e)
    //{
    //    await Shell.Current.GoToAsync("//ListPage");
    //}

    public async void SubmitFeatureClicked (object sender, EventArgs e)
    {
        var dic = new Dictionary<string,string>();
        dic["title"] = EventName.Text;
        dic["category"] = CategoryPicker.SelectedItem.ToString();
        dic["description"] = EventDescription.Text;

       
        AddFoodFeature(pinPoint, dic);

        await Navigation.PopAsync();
        //BackHome(sender, e);
    }

    public async void LoadFeatureTable()
    {
        await serviceGeodatabase.LoadAsync();
        foodFeatureTable = serviceGeodatabase.GetTable(0);
        await foodFeatureTable.LoadAsync();
        //foodFeatureTable.Loaded += FoodFeatureTable_Loaded;
        FoodFeatureTable_Loaded();

    }

    private void FoodFeatureTable_Loaded()
    {
        SubmitFeatureButton.IsEnabled = true;
    }

    public async void AddFoodFeature(MapPoint location, Dictionary<string, string> args)
    {
        ArcGISFeature feature = (ArcGISFeature)foodFeatureTable.CreateFeature();
        MapPoint tappedPoint = (MapPoint)location.NormalizeCentralMeridian();

        feature.Geometry = tappedPoint;
        feature.SetAttributeValue("Title", args["title"]);
        feature.SetAttributeValue("Categories", args["category"]);
        feature.SetAttributeValue("Description", args["description"]);

        await foodFeatureTable.AddFeatureAsync(feature);
        await serviceGeodatabase.ApplyEditsAsync();
        feature.Refresh();
        await Application.Current.MainPage.DisplayAlert("Success", $"Created feature {feature.Attributes["objectid"]}", "OK");
    }

}