using CollectionManager.Libraries;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace CollectionManager.Models
{
    [AddINotifyPropertyChangedInterface]
    internal class CollectionModel : IQueryAttributable
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public CustomColumnModel CurrentCustomColumn { get; set; }
        public ObservableCollection<CustomColumnModel> UserColumnNames { get; set; }
        public ObservableCollection<ItemModel> Items { get; set; }
        public ObservableCollection<CustomSelectItemModel> SelectItems { get; set; }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            Name = TextFileIOLibrary.ConvertSafeToText((string)query["Name"]);
        }
    }
}
