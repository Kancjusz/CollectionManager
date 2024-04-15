using CollectionManager.Views;

namespace CollectionManager
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            Routing.RegisterRoute("PickCollection", typeof(PickCollection));
            Routing.RegisterRoute("AddCollection", typeof(AddCollection));
            Routing.RegisterRoute("CollectionPage", typeof(CollectionPage));
            Routing.RegisterRoute("AddItem", typeof(AddItem));
            Routing.RegisterRoute("EditItem", typeof(EditItem));
            Routing.RegisterRoute("CollectionSummary", typeof(CollectionSummary));
        }
    }
}
