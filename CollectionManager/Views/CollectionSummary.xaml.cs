using CollectionManager.Libraries;
using CollectionManager.Models;
using System.Collections.ObjectModel;

namespace CollectionManager.Views;

public partial class CollectionSummary : ContentPage
{
	public CollectionSummary()
	{
		InitializeComponent();
        this.Loaded += CollectionSummary_Loaded;

    }

    ~CollectionSummary()
    {
        this.Loaded -= CollectionSummary_Loaded;
    }

    private async void CollectionSummary_Loaded(object? sender, EventArgs e)
    {
        await LoadCollectionSummary();
    }

    private async Task LoadCollectionSummary()
    {
        CollectionSummaryModel collectionSummaryModel = (CollectionSummaryModel)BindingContext;
        CollectionModel collectionModel = await TextFileIOLibrary.GetCollectionModelFromFile(collectionSummaryModel.Name + ".txt");
        BindingContext = GetCollectionSummaryModel(collectionModel);
    }

    private CollectionSummaryModel GetCollectionSummaryModel(CollectionModel collectionModel)
    {
        if (collectionModel.Items == null || collectionModel.Items.Count == 0) return new CollectionSummaryModel();
        
        CollectionSummaryModel model = new CollectionSummaryModel();

        model.Name = collectionModel.Name;
        model.ItemAmount = collectionModel.Items.Count;
        model.NewItemAmount = collectionModel.Items.Where(e=>e.Status==0).Count();
        model.WornItemAmount = collectionModel.Items.Where(e => e.Status == 1).Count();
        model.ForSaleItemAmount = collectionModel.Items.Where(e => e.Status == 2).Count();
        model.SoldItemAmount = collectionModel.Items.Where(e => e.Status == 3).Count();
        model.ToBuyItemAmount = collectionModel.Items.Where(e => e.Status == 4).Count();
        model.ItemPossessionAmount = model.WornItemAmount + model.NewItemAmount + model.ForSaleItemAmount;

        model.CheapestItemPrice = collectionModel.Items.MinBy(e => e.Price).Price;
        model.CheapestItemName = collectionModel.Items.MinBy(e => e.Price).Name;

        model.ExpensiveItemPrice = collectionModel.Items.MaxBy(e => e.Price).Price;
        model.ExpensiveItemName = collectionModel.Items.MaxBy(e => e.Price).Name;

        ObservableCollection<RatingModel> ratings = new ObservableCollection<RatingModel>();
        for(int i = 0; i < 11; i++)
        {
            int amount = collectionModel.Items.Where((e) => e.Rating == (byte)i).Count();
            ratings.Add(new RatingModel() { Name = i.ToString() + "/10", Amount = amount});
        }
        model.Ratings = ratings;

        ratingsView.ItemsSource = ratings;

        return model;
    }

    private async void GoBack_Clicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync($"../");
    }
}