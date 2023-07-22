using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using System.Formats.Asn1;

namespace Freefood;

public partial class FoodFormPage : ContentPage
{
    public List<string> FoodCategories = new List<string> { "Pizza", "Beverages", "Other" };
    private FeatureTable foodFeatureTable;
    ServiceGeodatabase serviceGeodatabase = new ServiceGeodatabase(MapViewModel.foodUri);

    public FoodFormPage()
	{
		InitializeComponent();
        CategoryPicker.ItemsSource = FoodCategories;
        LoadFeatureTable();
    }

    public async void BackHome(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//MainPage");
    }

    public async void SubmitFeatureClicked (object sender, EventArgs e)
    {
        var dic = new Dictionary<string,string>();
        dic["title"] = EventName.Text;
        dic["category"] = CategoryPicker.SelectedItem.ToString();
        dic["description"] = EventDescription.Text;

        var location = new MapPoint(0, 0);
        AddFoodFeature(location, dic);

        BackHome(sender, e);
    }

    public async void LoadFeatureTable()
    {
        await serviceGeodatabase.LoadAsync();
        foodFeatureTable = serviceGeodatabase.GetTable(0);
        foodFeatureTable.Loaded += FoodFeatureTable_Loaded;
    }

    private void FoodFeatureTable_Loaded(object sender, EventArgs e)
    {
        SubmitFeatureButton.IsEnabled = true;
    }

    public async void AddFoodFeature(MapPoint location, Dictionary<string, string> args)
    {
        ArcGISFeature feature = (ArcGISFeature)foodFeatureTable.CreateFeature();
        MapPoint tappedPoint = (MapPoint)location.NormalizeCentralMeridian();

        feature.Geometry = tappedPoint;
        feature.SetAttributeValue("Title", args["title"]);
        feature.SetAttributeValue("Category", args["category"]);
        //feature.SetAttributeValue("description", args["description"]);

        await foodFeatureTable.AddFeatureAsync(feature);
        await serviceGeodatabase.ApplyEditsAsync();
        feature.Refresh();
        await Application.Current.MainPage.DisplayAlert("Success", $"Created feature {feature.Attributes["objectid"]}", "OK");
    }

}