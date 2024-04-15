using CollectionManager.Models;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace CollectionManager.Libraries
{
    internal class TextFileIOLibrary
    {
        private static readonly string _collectionNameFilePath = "collections.txt";

        //FUNKCJE:
        //Dodanie pliku nazw kolekcji
        //Dodanie pliku kolekcji i wszystkich danych
        //Odczytanie danych z pliku kolekcji na podstawie id kolekcji

        private static void CreateFile(string targetFileName)
        {
            string targetFile = Path.Combine(FileSystem.Current.AppDataDirectory, targetFileName);
            File.Create(targetFile);
        }

        private static async Task ReplaceAllTextInFile(string text, string targetFileName)
        {
            string targetFile = Path.Combine(FileSystem.Current.AppDataDirectory, targetFileName);
            await File.WriteAllTextAsync(targetFile, string.Empty);

            FileStream outputStream = File.OpenWrite(targetFile);
            StreamWriter streamWriter = new StreamWriter(outputStream);
            await streamWriter.WriteAsync(text);
            streamWriter.Close();
        }

        private static async Task WriteTextToFile(string text, string targetFileName)
        {
            string targetFile = Path.Combine(FileSystem.Current.AppDataDirectory, targetFileName);
            FileStream outputStream = File.OpenWrite(targetFile);
            StreamWriter streamWriter = new StreamWriter(outputStream);
            await streamWriter.WriteAsync(text);
            streamWriter.Close();
        }

        private static async Task<string> ReadTextFile(string targetFileName)
        {
            string targetFile = Path.Combine(FileSystem.Current.AppDataDirectory, targetFileName);
            FileStream InputStream = File.OpenRead(targetFile);
            StreamReader reader = new StreamReader(InputStream);
            string contents = await reader.ReadToEndAsync();
            reader.Close();
            return contents;
        }

        private static async Task AddTextToFile(string text, string targetFileName)
        {
            string textInFile = await ReadTextFile(targetFileName);
            textInFile += text;
            await WriteTextToFile(textInFile, targetFileName);
        }

        public static async Task NewFileFromCollectionModel(CollectionModel model)
        {
            string ogName = model.Name;
            ConvertCollectionModelToSafe(ref model);

            string content = "";
            content += $"/{model.Id}\\/{model.Name}\\\n/[\n";


            foreach (ItemModel item in model.Items ?? new())
            {
                content += $"{{<{item.Id}>";
                content += $"<{item.Name}>";
                content += $"<{item.Comment}>";
                content += $"<{item.Price}>";
                content += $"<{item.Status}>";
                content += $"<{item.Rating}>";

                foreach (CustomColumnModel columnModel in item.UserColumns ?? new())
                {
                    content += $"<({columnModel.Id})";
                    content += $"({columnModel.Name})";
                    content += $"({columnModel.Value})>";
                }
                content += "}\n";
            }
            content += "]\\\n";

            content += "/[\n";
            foreach (CustomColumnModel columnModel1 in model.UserColumnNames ?? new())
            {
                content += $"{{<{columnModel1.Id}>";
                content += $"<{columnModel1.Name}>";

                if (columnModel1.Value is not ObservableCollection<CustomSelectItemModel>)
                {
                    content += $"<{columnModel1.Value}>";
                    content += "}\n";
                    continue;
                }

                ObservableCollection<CustomSelectItemModel> list = columnModel1.Value as ObservableCollection<CustomSelectItemModel>;
                content += "<(";
                foreach (CustomSelectItemModel customSelect in list)
                {
                    content += $"\"{customSelect.Name}\"";
                }

                content += ")>}\n";
            }
            content += "]\\";

            string targetFile = Path.Combine(FileSystem.Current.AppDataDirectory, model.Name + ".txt");
            Debug.WriteLine("Path: " + targetFile);

            await ReplaceAllTextInFile(content, targetFile);
        }

        public static async Task<CollectionModel> GetCollectionModelFromFile(string fileName)
        {
            string name = ConvertTextToSafe(fileName);
            string content = await ReadTextFile(name);

            content = content.Replace("\n", "");
            content = content.Replace("\\", "\\");

            string[] baseElements = Regex.Matches(content, @"\/.*?\\").Cast<Match>().Select(m => m.Value).ToArray();
            CollectionModel collectionModel = new CollectionModel()
            {
                Id = int.Parse(baseElements[0].Replace("/", "").Replace("\\", "")),
                Name = baseElements[1].Replace("/", "").Replace("\\", ""),
            };

            //Convert Items List
            string items = baseElements[2].Replace("/", "").Replace("\\", "").Replace("[", "").Replace("]", "");
            ObservableCollection<ItemModel> itemModels = new ObservableCollection<ItemModel>();

            if (items.Length == 0) goto skipItems;

            string[] itemElements = Regex.Matches(items, @"{.*?}").Cast<Match>().Select(m => m.Value).ToArray();
            foreach (string itemElement in itemElements)
            {
                string item = itemElement.Replace("{", "").Replace("}", "");
                string[] itemContents = Regex.Matches(item, @"<.*?>").Cast<Match>().Select(m => m.Value).ToArray();

                ItemModel itemModel = new ItemModel()
                {
                    Id = int.Parse(itemContents[0].Replace("<", "").Replace(">", "")),
                    Name = itemContents[1].Replace("<", "").Replace(">", ""),
                    Comment = itemContents[2].Replace("<", "").Replace(">", ""),
                    Price = float.Parse(itemContents[3].Replace("<", "").Replace(">", "")),
                    Rating = byte.Parse(itemContents[5].Replace("<", "").Replace(">", "")),
                    Status = int.Parse(itemContents[4].Replace("<", "").Replace(">", ""))
                };

                if (itemContents.Length == 6)
                {
                    itemModels.Add(itemModel);
                    continue;
                }
                ObservableCollection<CustomColumnModel> customColumns = new ObservableCollection<CustomColumnModel>();

                string[] columns = new string[itemContents.Length - 6];
                Array.Copy(itemContents, 6, columns, 0, columns.Length);

                foreach (string column in columns)
                {
                    string columnItem = column.Replace("<", "").Replace(">", "");
                    string[] columnItems = Regex.Matches(columnItem, @"\(.*?\)").Cast<Match>().Select(m => m.Value).ToArray();

                    customColumns.Add(new CustomColumnModel()
                    {
                        Id = int.Parse(columnItems[0].Replace("(", "").Replace(")", "")),
                        Name = columnItems[1].Replace("(", "").Replace(")", ""),
                        Value = columnItems[2].Replace("(", "").Replace(")", "")
                    });
                }
                itemModel.UserColumns = customColumns;
                itemModels.Add(itemModel);
            }

            skipItems:
            collectionModel.Items = itemModels;

            //CustomUserColumns
            string customUserColumns = baseElements[3].Replace("/", "").Replace("\\", "").Replace("[", "").Replace("]", "");
            ObservableCollection<CustomColumnModel> customColumnModels = new ObservableCollection<CustomColumnModel>();

            if (customUserColumns.Length == 0) goto skipColumns;

            string[] columnElements = Regex.Matches(customUserColumns, @"{.*?}").Cast<Match>().Select(m => m.Value).ToArray();
            foreach (string columnElement in columnElements)
            {
                string column = columnElement.Replace("{", "").Replace("}", "");
                string[] columnContents = Regex.Matches(column, @"<.*?>").Cast<Match>().Select(m => m.Value).ToArray();

                CustomColumnModel customColumnModel = new CustomColumnModel()
                {
                    Id = int.Parse(columnContents[0].Replace("<", "").Replace(">", "")),
                    Name = columnContents[1].Replace("<", "").Replace(">", "")
                };

                if (!Regex.IsMatch(columnContents[2], @"\(.*?\)"))
                {
                    customColumnModel.Value = columnContents[2].Replace("<", "").Replace(">", "");
                    customColumnModels.Add(customColumnModel);
                    continue;
                }

                ObservableCollection<CustomSelectItemModel> customSelects = new ObservableCollection<CustomSelectItemModel>();
                string select = columnContents[2].Replace("<", "").Replace(">", "").Replace("(", "").Replace(")", "");
                string[] selectItems = Regex.Matches(select, @""".*?""").Cast<Match>().Select(m => m.Value).ToArray();
                foreach (string selectItem in selectItems)
                {
                    customSelects.Add(new CustomSelectItemModel()
                    {
                        Name = selectItem.Replace("\"", ""),
                    });
                }
                customColumnModel.Value = customSelects;
                customColumnModels.Add(customColumnModel);
            }

        skipColumns:
            collectionModel.UserColumnNames = customColumnModels;

            ConvertCollectionModelToText(ref collectionModel);

            return collectionModel;
        }

        public static string ConvertTextToSafe(string text)
        {
            return text
                .Replace("_", "_prc")
                .Replace("/", "_fsl")
                .Replace("\\", "_bsl")
                .Replace("{", "_obc")
                .Replace("}", "_cbc")
                .Replace("[", "_osq")
                .Replace("]", "_csq")
                .Replace("(", "_ocr")
                .Replace(")", "_ccr")
                .Replace("<", "_otr")
                .Replace(">", "_ctr")
                .Replace("\"", "_qut");
        }

        public static string ConvertSafeToText(string text)
        {
            return text
                .Replace("_fsl", "/")
                .Replace( "_bsl", "\\")
                .Replace( "_obc", "{")
                .Replace( "_cbc", "}")
                .Replace( "_osq", "[")
                .Replace( "_csq", "]")
                .Replace( "_ocr", "(")
                .Replace( "_ccr", ")")
                .Replace( "_otr", "<")
                .Replace( "_ctr", ">")
                .Replace( "_qut", "\"")
                .Replace( "_prc", "_");
        }

        private static void ConvertCollectionModelToSafe(ref CollectionModel collectionModel)
        {
            collectionModel.Name = ConvertTextToSafe(collectionModel.Name);
            foreach(ItemModel itemModel in collectionModel.Items ?? new())
            {
                itemModel.Name = ConvertTextToSafe(itemModel.Name);
                itemModel.Comment = ConvertTextToSafe(itemModel.Comment);
                
                foreach(CustomColumnModel customColumn in itemModel.UserColumns ?? new())
                {
                    customColumn.Name = ConvertTextToSafe(customColumn.Name);
                    customColumn.Value = ConvertTextToSafe(customColumn.Value.ToString());
                }
            }

            foreach(CustomColumnModel customColumn in collectionModel.UserColumnNames ?? new())
            {
                customColumn.Name = ConvertTextToSafe(customColumn.Name);
                
                if(customColumn.Value is ObservableCollection<CustomSelectItemModel>)
                {
                    ObservableCollection<CustomSelectItemModel> customSelects = (ObservableCollection<CustomSelectItemModel>) customColumn.Value;
                    foreach(CustomSelectItemModel customSelectItemModel in customSelects)
                    {
                        customSelectItemModel.Name = ConvertTextToSafe(customSelectItemModel.Name);
                    }
                }
            }
        }

        private static void ConvertCollectionModelToText(ref CollectionModel collectionModel)
        {
            collectionModel.Name = ConvertSafeToText(collectionModel.Name);
            foreach (ItemModel itemModel in collectionModel.Items ?? new())
            {
                itemModel.Name = ConvertSafeToText(itemModel.Name);
                itemModel.Comment = ConvertSafeToText(itemModel.Comment);

                foreach (CustomColumnModel customColumn in itemModel.UserColumns ?? new())
                {
                    customColumn.Name = ConvertSafeToText(customColumn.Name);
                    customColumn.Value = ConvertSafeToText((string)customColumn.Value);
                }
            }

            foreach (CustomColumnModel customColumn in collectionModel.UserColumnNames ?? new())
            {
                customColumn.Name = ConvertSafeToText(customColumn.Name);

                if (customColumn.Value is ObservableCollection<CustomSelectItemModel>)
                {
                    ObservableCollection<CustomSelectItemModel> customSelects = (ObservableCollection<CustomSelectItemModel>)customColumn.Value;
                    foreach (CustomSelectItemModel customSelectItemModel in customSelects)
                    {
                        customSelectItemModel.Name = ConvertSafeToText(customSelectItemModel.Name);
                    }
                }
            }
        }

        public static void CreateCollectionsFile()
        {
            CreateFile(_collectionNameFilePath);
        }

        public static async Task AddCollection(CollectionModel model)
        {
            string name = ConvertTextToSafe(model.Name);
            await AddTextToFile("<"+name + ">", _collectionNameFilePath);
            await NewFileFromCollectionModel(model);
        }

        public static async Task<string[]> GetCollectionNameList()
        {
            string content = await ReadTextFile(_collectionNameFilePath);
            string[] collections = Regex.Matches(content, @"<.*?>").Cast<Match>().Select(m => m.Value).ToArray();
            collections = collections.Select(e=>e.Replace("<","").Replace(">","")).ToArray();

            collections = collections.Select(ConvertSafeToText).ToArray();

            return collections;
        }
    }
}
