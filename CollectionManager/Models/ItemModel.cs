using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CollectionManager.Libraries;
using PropertyChanged;

namespace CollectionManager.Models
{
    public enum ItemStatus { New, Worn, ForSale, Sold, ToBuy }

    [AddINotifyPropertyChangedInterface]
    internal class ItemModel : IQueryAttributable
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Comment { get; set; }
        public float Price { get; set; }
        public int Status { get; set; }
        public byte Rating { get; set; }
        public ObservableCollection<CustomColumnModel> UserColumns { get; set; }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            Name = TextFileIOLibrary.ConvertSafeToText((string)query["Name"]);
            Id = int.Parse((string)query["Id"]);
        }
    }
}
