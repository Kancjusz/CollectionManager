using CollectionManager.Libraries;
using CollectionManager.Models;
using System.Diagnostics;

namespace CollectionManager.Views;

public partial class PickCollection : ContentPage
{
    public PickCollection()
    {
        InitializeComponent();
        LoadList();
        Debug.WriteLine("Path: " + FileSystem.Current.AppDataDirectory);
    }

    private async void LoadList()
    {
        try {
            string[] collections = await TextFileIOLibrary.GetCollectionNameList();

            List<CollectionModel> collectionsList = new List<CollectionModel>();

            int i = 0;
            foreach (string collection in collections)
            {
                collectionsList.Add(new CollectionModel { Name = collection, Id = i});
                i++;
            }

            BindingContext = new CollectionListModel()
            {
                CollectionList = collectionsList
            };
        } catch (FileNotFoundException ex) {
            TextFileIOLibrary.CreateCollectionsFile();

            BindingContext = new CollectionListModel();
        }

    }

    private async void AddNewCollection_Clicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync($"{nameof(AddCollection)}?Name={""}");
    }

    private async void ClassCollection_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        int id = (e.CurrentSelection.FirstOrDefault() as CollectionModel).Id;
        CollectionModel model = ((CollectionListModel)BindingContext).CollectionList[id];

        await Shell.Current.GoToAsync($"{nameof(CollectionPage)}?Name={TextFileIOLibrary.ConvertTextToSafe(model.Name)}");
    }
}