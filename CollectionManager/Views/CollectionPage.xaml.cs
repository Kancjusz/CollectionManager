using CollectionManager.Libraries;
using CollectionManager.Models;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Xml.Linq;

namespace CollectionManager.Views;

public partial class CollectionPage : ContentPage
{
    public CollectionPage()
    {
        InitializeComponent();
        this.Loaded += CollectionPage_Loaded;
    }

    ~CollectionPage()
    {
        this.Loaded -= CollectionPage_Loaded;
    }

    private void CollectionPage_Loaded(object? sender, EventArgs e)
    {
        LoadCollection();
    }

    private async void LoadCollection()
    {
        CollectionModel collectionModel = (CollectionModel)BindingContext;
        collectionModel = await TextFileIOLibrary.GetCollectionModelFromFile(collectionModel.Name + ".txt");
        BindingContext = collectionModel;

        AdjustItemListViewWidth(collectionModel.UserColumnNames.Count);
        SoldItemsToBottom();
    }

    private void SoldItemsToBottom()
    {
        CollectionModel collectionModel = (CollectionModel)BindingContext;
        ObservableCollection<ItemModel> items = new ObservableCollection<ItemModel>();

        List<ItemModel> list = collectionModel.Items.Where(e => e.Status != 3).ToList().Concat(collectionModel.Items.Where(e => e.Status == 3).ToList()).ToList() ?? new();
        items = new ObservableCollection<ItemModel>(list);

        itemListView.ItemsSource = items;
    }

    private async void BackButton_Clicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync($"{nameof(PickCollection)}");
    }

    private async void AddNewItem_Clicked(object sender, EventArgs e)
    {
        CollectionModel model = (CollectionModel)BindingContext;

        await Shell.Current.GoToAsync($"{nameof(AddItem)}?Name={TextFileIOLibrary.ConvertTextToSafe(model.Name)}&Id={0}");
    }

    private void ItemRating_Loaded(object sender, EventArgs e)
    {
        Label label = (Label)sender;
        ItemModel model = (ItemModel)label.BindingContext;

        label.Text = model.Rating.ToString()+"/10";
    }

    private void ItemStatus_Loaded(object sender, EventArgs e)
    {
        Label label = (Label)sender;
        ItemModel model = (ItemModel)label.BindingContext;

        label.Text = ((ItemStatus)model.Status).ToString();
    }

    private void CustomColumn_Loaded(object sender, EventArgs e)
    {
        Label label = (Label)sender;
        CustomColumnModel model = (CustomColumnModel)label.BindingContext;
        CollectionModel collectionModel = (CollectionModel)BindingContext;

        object value = collectionModel.UserColumnNames[model.Id].Value;
        if (value is not ObservableCollection<CustomSelectItemModel>) return;

        ObservableCollection<CustomSelectItemModel> customSelects = (ObservableCollection<CustomSelectItemModel>)value;
        string sIndex = (string)model.Value;
        string selectName = customSelects[int.Parse(sIndex)].Name;

        label.Text = selectName;
    }

    private void ItemLayout_Loaded(object sender, EventArgs e)
    {
        FlexLayout layout = (FlexLayout)sender;
        ItemModel itemModel = (ItemModel)layout.BindingContext;

        if (itemModel.Status != 3) return;

        List<IVisualTreeElement> layoutList = (List<IVisualTreeElement>)layout.GetVisualTreeDescendants();

        foreach(IVisualTreeElement view in layoutList)
        {
            if(view is Label)
            {
                Label label = (Label)view;
                label.TextColor = Color.FromArgb("#FF3B3334");
            }

            if(view is CollectionView)
            {
                List<IVisualTreeElement> viewList = (List<IVisualTreeElement>)view.GetVisualTreeDescendants();

                foreach(IVisualTreeElement element in viewList)
                {
                    if(element is Label)
                    {
                        Label label = (Label)element;
                        label.TextColor = Color.FromArgb("#FF3B3334");
                    }
                }
            }

            if(view is Button)
            {
                Button button = (Button)view;
                button.BackgroundColor = Color.FromArgb("#FF777777");
                button.TextColor = Color.FromArgb("#FF534849");
            }
        }

        layout.BackgroundColor = Color.FromArgb("#FF4C505C");
    }

    private void AdjustItemListViewWidth(int columnsAmount)
    {
        if (columnsAmount <= 5) return;

        columnsAmount -= 5;

        itemListView.WidthRequest = 1100 + 100 * columnsAmount;
    }

    private async void ItemEdit_Clicked(object sender, EventArgs e)
    {
        Button button = (Button)sender;
        ItemModel model = (ItemModel)button.BindingContext;
        CollectionModel collectionModel = (CollectionModel)BindingContext;

        await Shell.Current.GoToAsync($"{nameof(EditItem)}?Name={TextFileIOLibrary.ConvertTextToSafe(collectionModel.Name)}&Id={model.Id}");
    }

    private async void ItemDelete_Clicked(object sender, EventArgs e)
    {
        Button button = (Button)sender;
        ItemModel model = (ItemModel)button.BindingContext;
        CollectionModel collectionModel = (CollectionModel)BindingContext;

        collectionModel.Items.RemoveAt(model.Id);
        collectionModel.Items = new ObservableCollection<ItemModel>(collectionModel.Items.Select((e, i) => { e.Id = i; return e; }).ToList());

        BindingContext = collectionModel;

        SoldItemsToBottom();

        await TextFileIOLibrary.NewFileFromCollectionModel(collectionModel);
    }

    private async void CollectionSummary_Clicked(object sender, EventArgs e)
    {
        CollectionModel collectionModel = (CollectionModel)BindingContext;
        await Shell.Current.GoToAsync($"{nameof(CollectionSummary)}?Name={TextFileIOLibrary.ConvertTextToSafe(collectionModel.Name)}");
    }
}