using Esri.ArcGISRuntime.Data;

namespace Freefood;

public partial class FeaturePage : ContentPage
{
	private ArcGISFeature feature;
    private IReadOnlyList<Attachment> attachments;
    
	public FeaturePage(ArcGISFeature feature)
	{
		InitializeComponent();
		this.feature = feature;
		this.BindingContext = this;

        attachments = feature.GetAttachmentsAsync().Result;
	}

    public string EventTitle
    {
        get => feature.Attributes["Title"].ToString();
    }
    public string EventDescription
    {
        get => "Description: "+ feature.Attributes["Description"].ToString();
    }
    public string EventStartDate
    {
        get => "Posted: " + feature.Attributes["StartDate"].ToString();
    }

    public string EventNavigationDetails
    {
        get => "Directions: " + feature.Attributes["NavigationDetails"].ToString();
    }

    public ImageSource EventImage
    {
        get => ImageSource.FromStream(() => attachments.First().GetDataAsync().Result);
    }

}