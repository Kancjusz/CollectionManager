using CollectionManager.Libraries;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollectionManager.Models
{
    [AddINotifyPropertyChangedInterface]
    internal class CollectionSummaryModel : IQueryAttributable
    {
        public string Name { get; set; }
        public int ItemAmount { get; set; }
        public int ItemPossessionAmount { get; set; }
        public int NewItemAmount { get; set; }
        public int WornItemAmount { get; set; }
        public int ForSaleItemAmount { get; set; }
        public int SoldItemAmount { get; set; }
        public int ToBuyItemAmount { get; set; }

        public string CheapestItemName { get; set; }
        public float CheapestItemPrice { get; set; }

        public string ExpensiveItemName { get; set; }
        public float ExpensiveItemPrice { get; set; }

        public ObservableCollection<RatingModel> Ratings { get; set; }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            Name = TextFileIOLibrary.ConvertSafeToText((string)query["Name"]);
        }
    }
}
