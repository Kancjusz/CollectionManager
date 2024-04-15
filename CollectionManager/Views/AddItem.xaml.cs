using CollectionManager.Libraries;
using CollectionManager.Models;
using System.Collections.ObjectModel;

namespace CollectionManager.Views;

public partial class AddItem : ContentPage
{
    CollectionModel collectionModel;

	public AddItem()
	{
		InitializeComponent();
        this.Loaded += AddItem_Loaded;
	}

    ~AddItem()
    {
        this.Loaded -= AddItem_Loaded;
    }

    private async void AddItem_Loaded(object? sender, EventArgs e)
    {
        await LoadItem();
    }

    private async Task LoadItem()
    {
        ItemModel itemModel = (ItemModel)BindingContext;
        collectionModel = await TextFileIOLibrary.GetCollectionModelFromFile(itemModel.Name + ".txt");

        ObservableCollection<CustomColumnModel> list = new ObservableCollection<CustomColumnModel>();
        foreach (CustomColumnModel item in collectionModel.UserColumnNames)
        {
            CustomColumnModel model1 = new CustomColumnModel();
            model1.Id = item.Id;
            model1.Name = item.Name;

            if(model1.Value is not ObservableCollection<CustomSelectItemModel>) 
                model1.Value = "";
            else
            {
                ObservableCollection<CustomSelectItemModel> selects = new ObservableCollection<CustomSelectItemModel>();
                foreach(CustomSelectItemModel customSelectItemModel in (ObservableCollection<CustomSelectItemModel>)item.Value)
                {
                    selects.Add(new CustomSelectItemModel() { Name = customSelectItemModel.Name });
                }
                model1.Value = selects;
            }
            list.Add(model1);
        }

        BindingContext = new ItemModel()
        {
            Name = "",
            Comment = "",
            Price = 0,
            Status = 0,
            Rating = 0,
            UserColumns = list
        };

    }

    private async void CancelButton_Clicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }

    private async void AddItem_Clicked(object sender, EventArgs e)
    {
        ItemModel model = (ItemModel)BindingContext;
        model.Id = collectionModel.Items.Count;

        foreach (ItemModel item in collectionModel.Items)
        {
            if (item.Name == model.Name)
            {
                bool result = await DisplayAlert("Alert", "There already is an item with this name. Do you still want to add this item?", "Yes", "No");
                if (result) break;
                else return;
            }
        }

        collectionModel.Items.Add(model);

        await TextFileIOLibrary.NewFileFromCollectionModel(collectionModel);
        await Shell.Current.GoToAsync($"{nameof(CollectionPage)}?Name={collectionModel.Name}");
    }

    private void CustomColumnInput_Loaded(object sender, EventArgs e)
    {
        Grid frame = (Grid)sender;
        CustomColumnModel model = (CustomColumnModel)frame.BindingContext;

        CustomColumnModel columnModel = collectionModel.UserColumnNames.Where(e => e.Id == model.Id).Single();

        frame.Children.Clear();

        frame.Add(columnModel.Value switch
        {
            0 or "0" => DynamicInputsLibrary.CreateCustomColumnEntry(model, false, frame),
            1 or "1" => DynamicInputsLibrary.CreateCustomColumnEntry(model, true, frame),
            _ => DynamicInputsLibrary.CreateCustomColumnPicker(model, columnModel, frame)
        }, 0, 0);
    }

    private void ItemPriceText_Changed(object sender, TextChangedEventArgs e)
    {
        Editor editor = (Editor)sender;
        editor.Text = DynamicInputsLibrary.ValidateNumberMoreOrEqualZero(e);
    }
}