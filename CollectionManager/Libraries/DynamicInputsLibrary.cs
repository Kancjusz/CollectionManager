using CollectionManager.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollectionManager.Libraries
{
    internal class DynamicInputsLibrary
    {
        public static Entry CreateCustomColumnEntry(CustomColumnModel model, bool isNumber, Grid grid)
        {
            Entry entry = new Entry();

            entry.Keyboard = isNumber ? Keyboard.Numeric : Keyboard.Default;
            if(isNumber )
            {
                entry.TextChanged += Entry_TextChanged;
            }

            entry.HorizontalOptions = LayoutOptions.Center;
            entry.WidthRequest = grid.Width;
            entry.HeightRequest = grid.Height;
            entry.Margin = new Thickness(0,0,0,0);
            entry.HorizontalTextAlignment = TextAlignment.Center;
            entry.IsVisible = true;

            entry.BindingContext = model;

            entry.SetBinding(Entry.TextProperty, "Value");

            return entry;
        }

        private static void Entry_TextChanged(object? sender, TextChangedEventArgs e)
        {
            Entry entry = (Entry)sender;
            entry.Text = ValidateAnyNumber(e);
        }

        public static Picker CreateCustomColumnPicker(CustomColumnModel model, CustomColumnModel columnModel, Grid grid)
        {
            Picker picker = new Picker();

            ObservableCollection<CustomSelectItemModel> selectItemModels = (ObservableCollection<CustomSelectItemModel>)columnModel.Value;
            string[] list = new string[selectItemModels.Count];
            for (int i = 0; i < selectItemModels.Count; i++)
            {
                list[i] = selectItemModels[i].Name;
            }

            picker.ItemsSource = list;
            picker.WidthRequest = grid.Width;
            picker.Margin = 0;
            picker.HorizontalTextAlignment = TextAlignment.Center;
            picker.IsVisible = true;

            picker.BindingContext = model;

            picker.SetBinding(Picker.SelectedIndexProperty, "Value");
            picker.SelectedIndex = 0;

            return picker;
        }


        public static string ValidateNumberMoreOrEqualZero(TextChangedEventArgs e)
        {
            string newText = e.NewTextValue;
            string oldText = e.OldTextValue;

            if (oldText == "") return newText;
            if (newText == "") return oldText;

            if (oldText != null && newText == ("0" + oldText)) return oldText;

            if (float.TryParse(newText, out float value))
            {
                if (value < 0)
                    return oldText;
                return newText;
            }
            else return oldText;
        }

        public static string ValidateAnyNumber(TextChangedEventArgs e)
        {
            string newText = e.NewTextValue;
            string oldText = e.OldTextValue;

            if (oldText == "") return newText;
            if (newText == "") return oldText;

            if (oldText != null && newText == ("0" + oldText)) return oldText;
            if(oldText != null && newText.StartsWith("0") && newText.Length > 1 && oldText == "0") return newText.Substring(1);

            if (float.TryParse(newText, out float value))
                return newText;
            else return oldText;
        }
    }
}
