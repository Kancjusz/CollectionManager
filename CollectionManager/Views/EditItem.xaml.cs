using CollectionManager.Libraries;
using CollectionManager.Models;
using System.Collections.ObjectModel;

namespace CollectionManager.Views;

public partial class EditItem : ContentPage
{
    CollectionModel collectionModel;

    public EditItem()
    {
        InitializeComponent();
        this.Loaded += AddItem_Loaded;
    }

    ~EditItem()
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
        itemModel = collectionModel.Items[itemModel.Id];
        BindingContext = itemModel;
    }

    private async void CancelButton_Clicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }

    private async void EditItem_Clicked(object sender, EventArgs e)
    {
        ItemModel model = (ItemModel)BindingContext;

        foreach (ItemModel item in collectionModel.Items)
        {
            if (item.Name == model.Name && item.Id != model.Id)
            {
                bool result = await DisplayAlert("Alert", "There already is an item with this name. Do you still want to edit this item?", "Yes", "No");
                if (result) break;
                else return;
            }
        }

        collectionModel.Items[model.Id] = model;

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