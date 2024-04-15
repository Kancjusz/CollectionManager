using CollectionManager.Libraries;
using CollectionManager.Models;
using Microsoft.Maui.Controls;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reflection;
using System.Xml.Linq;

namespace CollectionManager.Views;

public partial class AddCollection : ContentPage
{
	public AddCollection()
	{
		InitializeComponent();
        CollectionModel model = (CollectionModel)BindingContext;
        model.CurrentCustomColumn = new CustomColumnModel()
        {
            Value = "0",
            Name = "",
            Id = 0
        };
        BindingContext = model;

    }

    private async void AddCollection_Clicked(object sender, EventArgs e)
    {
		CollectionModel model = (CollectionModel)BindingContext;

        if(model.Name == "")
        {
            await DisplayAlert("Alert", "Collection name can't be empty!", "Ok");
            return;
        }

        string[] collectionNames = await TextFileIOLibrary.GetCollectionNameList();
        if (collectionNames.Contains(model.Name))
        {
            await DisplayAlert("Alert","Collection with this name already exists!","Ok");
            return;
        }

        foreach (ItemModel item in model.Items ?? new())
        {
            if(model.Items.Where(e=>e.Name==item.Name).Count()>1)
            {
                bool result = await DisplayAlert("Alert", "Some items have the same names. Do you still want to create the collection?", "Yes","No");
                if (result) break;
                else return;
            }
        }

        await TextFileIOLibrary.AddCollection(model);

        await Shell.Current.GoToAsync($"{nameof(PickCollection)}");
    }

    private async void CancelButton_Clicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync($"../");
    }

    private async void AddCustomColumn_Clicked(object sender, EventArgs e)
    {
        CollectionModel model = (CollectionModel)BindingContext;

        model.UserColumnNames ??= new ObservableCollection<CustomColumnModel>();
        int length = model.UserColumnNames.Count;

        if(model.CurrentCustomColumn.Value is ObservableCollection<CustomSelectItemModel>)
        {
            ObservableCollection<CustomSelectItemModel> customSelects = (ObservableCollection<CustomSelectItemModel>)model.CurrentCustomColumn.Value;
            if(customSelects.Count() <= 1)
            {
                await DisplayAlert("Alert", "Custom list column has to have at least 2 items!", "Ok");
                return;
            }
        }

        model.UserColumnNames.Add(new CustomColumnModel() { Id = length, Name = model.CurrentCustomColumn.Name, Value = model.CurrentCustomColumn.Value } );

        BindingContext = model;

        SetAllItemsUserColumns();

        AdjustItemListViewWidth(model.UserColumnNames.Count);
    }

    private void AdjustItemListViewWidth(int columnsAmount)
    {
        if (columnsAmount <= 5) return;

        columnsAmount -= 5;

        itemListView.WidthRequest = 1000 + 100 * columnsAmount;
    }

    private void SetAllItemsUserColumns()
    {
        CollectionModel model = (CollectionModel)BindingContext;

        foreach(ItemModel itemModel in model.Items ?? new())
        {
            ObservableCollection<CustomColumnModel> list = new ObservableCollection<CustomColumnModel>();
            foreach (CustomColumnModel item in model.UserColumnNames ?? new())
            {
                CustomColumnModel model1 = new CustomColumnModel();
                model1.Id = item.Id;
                model1.Name = item.Name;
                model1.Value = item.Value;
                list.Add(model1);
            }
            itemModel.UserColumns = list;
        }

        BindingContext = model;
    }

    private void AddItem_Clicked(object sender, EventArgs e)
    {
        CollectionModel model = (CollectionModel)BindingContext;

        model.Items ??= new ObservableCollection<ItemModel>();
        int length = model.Items.Count;

        ObservableCollection<CustomColumnModel> list = new ObservableCollection<CustomColumnModel>();
        foreach(CustomColumnModel item in model.UserColumnNames ?? new())
        {
            CustomColumnModel model1 = new CustomColumnModel();
            model1.Id = item.Id;
            model1.Name = item.Name;
            model1.Value = item.Value;
            list.Add(model1);
        }

        model.Items.Add(new ItemModel() { Id = length, Name = "", Comment = "", Price = 0, Rating=0, Status=0, UserColumns=list });

        ItemListFrame.IsVisible = true;

        BindingContext = model;
    }

    private void CustomColumnInput_Loaded(object sender, EventArgs e)
    {
        Grid frame = (Grid)sender;
        CustomColumnModel model = (CustomColumnModel)frame.BindingContext;
        CollectionModel collectionModel = (CollectionModel)BindingContext;
        
        if(model.Value is ObservableCollection<CustomSelectItemModel>)
            model.Value = "0";

        CustomColumnModel columnModel = collectionModel.UserColumnNames.Where(e=>e.Id==model.Id).Single();

        frame.Children.Clear();



        frame.Add(columnModel.Value switch
        {
            null or 0 or "0" => DynamicInputsLibrary.CreateCustomColumnEntry(model,false,frame),
            1 or "1" => DynamicInputsLibrary.CreateCustomColumnEntry(model,true,frame),
            _ => DynamicInputsLibrary.CreateCustomColumnPicker(model, columnModel,frame)
        },0,0);
    }    

    private void ColumnTypeIndex_Changed(object sender, EventArgs e)
    {
        Picker picker = (Picker)sender;
        CollectionModel model = (CollectionModel)BindingContext;
        if (model.CurrentCustomColumn == null)
        {
            model.CurrentCustomColumn = new CustomColumnModel()
            {
                Value = "0",
                Name = "",
                Id = 0
            };
            BindingContext = model;
            return;
        }

        if (picker.SelectedIndex == -1)
        {
            picker.SelectedIndex = 0;
            model.CurrentCustomColumn.Value = picker.SelectedIndex;
            customSelectLayout.IsVisible = false;
            BindingContext = model;

            return;
        }

        if (picker.SelectedIndex == 0 || picker.SelectedIndex == 1)
        {
            model.CurrentCustomColumn.Value = picker.SelectedIndex;
            customSelectLayout.IsVisible = false;
            BindingContext = model;

            return;
        }

        ObservableCollection<CustomSelectItemModel> customSelectItems = new ObservableCollection<CustomSelectItemModel>();
        customSelectItems.Add(new CustomSelectItemModel() { Name = "" });

        model.SelectItems = new ObservableCollection<CustomSelectItemModel>(customSelectItems);
        model.CurrentCustomColumn.Value = new ObservableCollection<CustomSelectItemModel>(customSelectItems);
        customSelectLayout.IsVisible = true;

        BindingContext = model;
    }

    private void AddCustomSelect_Clicked(object sender, EventArgs e)
    {
        CollectionModel model = (CollectionModel)BindingContext;
        ObservableCollection<CustomSelectItemModel> list = model.SelectItems;
        list.Add(new CustomSelectItemModel() {Name="" });

        model.SelectItems = new ObservableCollection<CustomSelectItemModel>(list);
        model.CurrentCustomColumn.Value = new ObservableCollection<CustomSelectItemModel>(list);

        BindingContext = model;
    }

    private void ItemPriceText_Changed(object sender, TextChangedEventArgs e)
    {
        Editor editor = (Editor)sender;
        editor.Text = DynamicInputsLibrary.ValidateNumberMoreOrEqualZero(e);
    }
}