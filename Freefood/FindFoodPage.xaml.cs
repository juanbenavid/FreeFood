using Esri.ArcGISRuntime.Location;

namespace Freefood;

public partial class FindFoodPage : ContentPage
{
    private SystemLocationDataSource locationSource = new SystemLocationDataSource();
    public FindFoodPage()
	{
		InitializeComponent();
        this.BindingContext = new FindFoodViewModel(this);
    }
}