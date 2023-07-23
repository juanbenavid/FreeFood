using Esri.ArcGISRuntime.Data;

namespace Freefood;

public partial class FeaturePage : ContentPage
{
	private ArcGISFeature feature;
    private IReadOnlyList<Attachment> attachments;
    private double X;
    private double Y;
    
	public FeaturePage(ArcGISFeature feature, double X, double Y)
	{
        InitializeComponent();
        this.feature = feature;
        this.X = X;
        this.Y = Y;
        this.BindingContext = this;
        SetImageSource();

    }

    public string EventTitle
    {
        get => feature.Attributes["Title"]?.ToString() ?? " ";
    }
    public string EventDescription
    {
        get => "Description: "+ feature.Attributes["Description"]?.ToString() ?? " ";
    }
    public string EventStartDate
    {
        get => "Posted: " + feature.Attributes["StartDate"]?.ToString() ?? " ";
    }

    public string EventNavigationDetails
    {
        get => "Directions: " + feature.Attributes["NavigationDetails"]?.ToString() ?? " ";
    }

    public async Task SetImageSource()
    {
        attachments = feature.GetAttachmentsAsync().Result;
        if (attachments.Any())
        {
            EventImage.Source = ImageSource.FromStream(() => attachments.First().GetDataAsync().Result);
        }
    }

    private void Btn_clickedStartRouteNavigation(object sender, EventArgs e)
    {
        Navigation.PushAsync(new RouteNavigationPage(this.X, this.Y));
    }

}