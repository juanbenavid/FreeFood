using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using System.Formats.Asn1;
using System.Net.Mime;

namespace Freefood;

public partial class FoodFormPage : ContentPage
{
    public List<string> FoodCategories = new List<string>
    { "Pizza", "Beverages", "Candy", "Catered Food", "Chips", "Desserts", "Fruit", "Other", "Salad", "Sandwiches", "Snacks" };


    private FeatureTable foodFeatureTable;
    ServiceGeodatabase serviceGeodatabase = new ServiceGeodatabase(ListMapViewModel.foodUri);
    private MapPoint pinPoint;
    private byte[] attachmentData;
    private string filename;
    private string contentType = "image/jpeg";

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
        var dic = new Dictionary<string,dynamic>();
        dic["Title"] = EventName.Text;
        dic["Categories"] = CategoryPicker.SelectedItem.ToString();
        dic["Description"] = EventDescription.Text;
        dic["NavigationDetails"] = EventDirections.Text;
        dic["Donation"] = DonationCheck.IsChecked ? "Yes" : "No";
        dic["StartDate"] = DateTime.Now.ToString("yyyy-mm-dd hh:mm:ss tt");
        dic["EndDate"] = DateTime.Now.AddDays(1).ToString("yyyy-mm-dd hh:mm:ss tt");

       
        AddFoodFeature(pinPoint, dic);

        await Navigation.PopAsync();
       
    }

    public async void AttachPictureClicked(object sender, EventArgs e)
    {
        FileResult fileData = await FilePicker.PickAsync(new PickOptions { FileTypes = FilePickerFileType.Jpeg });
        if (fileData == null)
        {
            return;
        }
        if (!fileData.FileName.EndsWith(".jpg") && !fileData.FileName.EndsWith(".jpeg"))
        {
            await Application.Current.MainPage.DisplayAlert("Try again!", "This sample only allows uploading jpg files.", "OK");
            return;
        }

        using (Stream fileStream = await fileData.OpenReadAsync())
        {
            using (var memoryStream = new MemoryStream())
            {
                await fileStream.CopyToAsync(memoryStream);
                attachmentData = memoryStream.ToArray();
            }
        }
        filename = fileData.FileName;
    }

    public async void LoadFeatureTable()
    {
        await serviceGeodatabase.LoadAsync();
        foodFeatureTable = serviceGeodatabase.GetTable(0);
        await foodFeatureTable.LoadAsync();
        FoodFeatureTable_Loaded();

    }

    private void FoodFeatureTable_Loaded()
    {
        SubmitFeatureButton.IsEnabled = true;
    }

    public async void AddFoodFeature(MapPoint location, Dictionary<string, dynamic> args)
    {
        ArcGISFeature feature = (ArcGISFeature)foodFeatureTable.CreateFeature();
        MapPoint tappedPoint = (MapPoint)location.NormalizeCentralMeridian();

        feature.Geometry = tappedPoint;
        foreach (var pair in args)
        {
            feature.SetAttributeValue(pair.Key, pair.Value);
        }
        await feature.AddAttachmentAsync(filename, contentType, attachmentData);

        await foodFeatureTable.AddFeatureAsync(feature);
        await serviceGeodatabase.ApplyEditsAsync();
        feature.Refresh();
        await Application.Current.MainPage.DisplayAlert("Success", $"Created feature {feature.Attributes["objectid"]}", "OK");
    }

}