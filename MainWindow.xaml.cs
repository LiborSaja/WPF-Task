using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;
using Task.Model;
using Task.View;

namespace Task {
    
    public partial class MainWindow : Window, INotifyPropertyChanged {
        //seznam všech položek, import + manuální přidání
        private List<CarSales> carSalesList = new List<CarSales>();
        // Kolekce vybraných modelů a dnů
        private List<string> selectedModels = new List<string>();
        private List<DayOfWeek> selectedDays = new List<DayOfWeek> { DayOfWeek.Saturday, DayOfWeek.Sunday }; // Výchozí nastavení pro So a Ne
        // Filtrovaný seznam pro DataGrid
        private List<CarSales> filteredSalesList = new List<CarSales>();

        public MainWindow() {
            InitializeComponent();
            DataContext = this;

            // Nastavení DataGridu se seznamem carSalesList
            SalesDataGrid.ItemsSource = carSalesList;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void BtnAddItem_Click(object sender, RoutedEventArgs e) {
            // Vytvořit a zobrazit okno pro přidání položky
            var addItemWindow = new AddItemWindow();
            addItemWindow.ShowDialog();

            // Pokud byla položka přidána, přidáme ji do seznamu a aktualizujeme DataGrid
            if (addItemWindow.IsItemAdded && addItemWindow.NewCarSale != null)
            {
                carSalesList.Add(addItemWindow.NewCarSale);
                SalesDataGrid.ItemsSource = null; // Obnovíme zdroj dat
                SalesDataGrid.ItemsSource = carSalesList;

                // Aktualizace ListBoxu
                UpdateModelListBox();
            }
        }


    //Import XML souboru
    private void BtnImportFile_Click(object sender, RoutedEventArgs e) {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Vložení XML souboru | *.xml";
            ofd.Title = "Vložte, prosím, soubor XML.";
            ofd.Multiselect = false;

            bool? succes = ofd.ShowDialog();
            if(succes == true) {
                // Načti data z XML souboru
                var modelSales = LoadDataFromXml(ofd.FileName);
                carSalesList.AddRange(modelSales);

                // Obnovíme DataGrid, aby zobrazil aktualizovaný seznam
                SalesDataGrid.ItemsSource = null;
                SalesDataGrid.ItemsSource = carSalesList;

                // Aktualizace ListBoxu
                UpdateModelListBox();
            }
        }

        private List<CarSales> LoadDataFromXml(string filePath) {
            XDocument xdoc = XDocument.Load(filePath);
            var carSales = xdoc.Descendants("CarSale").Select(x => new CarSales {
                Model = x.Element("Model")?.Value ?? "Neznámý model",
                Date = DateTime.TryParse(x.Element("Date")?.Value, out DateTime saleDate) ? saleDate : DateTime.MinValue,
                Price = double.TryParse(x.Element("Price")?.Value, out double price) ? price : 0.0,
                DPH = double.TryParse(x.Element("DPH")?.Value, out double dph) ? dph : 0.0
            }).ToList();

            return carSales;
        }




        // Odstranění položek
        private void BtnDelete_Click(object sender, RoutedEventArgs e) {
            // Zkontrolujeme, zda jsou nějaké položky vybrány
            var selectedItems = SalesDataGrid.SelectedItems.Cast<CarSales>().ToList();

            if (selectedItems.Count > 0) {
                // Potvrzení od uživatele před smazáním
                var result = MessageBox.Show("Opravdu chcete smazat vybrané položky?", "Potvrzení smazání", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes) {
                    // Odstraníme vybrané položky ze seznamu carSalesList
                    foreach (var item in selectedItems) {
                        carSalesList.Remove(item);
                    }

                    // Obnovíme DataGrid
                    SalesDataGrid.ItemsSource = null;
                    SalesDataGrid.ItemsSource = carSalesList;

                    ApplyFilters();
                    UpdateModelListBox();
                }
            }
            else {
                MessageBox.Show("Žádné položky nejsou vybrány.", "Chyba", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        //ListBox pro výběr modelu
        private void UpdateModelListBox() {
            // Získáme unikátní názvy modelů
            var modelNames = carSalesList
                .GroupBy(car => car.Model.ToLower())   // Case-insensitive seskupení názvů modelů
                .Select(group => group.First().Model)  // Použijeme původní název modelu
                .ToList();

            // Přidáme položku "Všechny modely" na začátek
            modelNames.Insert(0, "Všechny modely");

            // Nastavení datového zdroje pro ListBox
            ModelListBox.ItemsSource = modelNames;

            // Automaticky vybereme položku "Všechny modely"
            ModelListBox.SelectedItems.Clear();
            ModelListBox.SelectedItems.Add("Všechny modely");
        }

        //výběr dle dnů
        private void DayCheckBox_Changed(object sender, RoutedEventArgs e) {
            var checkBox = sender as CheckBox;

            // Mapování českých názvů na hodnoty DayOfWeek
            var dayMapping = new Dictionary<string, DayOfWeek> {
                { "Po", DayOfWeek.Monday },
                { "Út", DayOfWeek.Tuesday },
                { "St", DayOfWeek.Wednesday },
                { "Čt", DayOfWeek.Thursday },
                { "Pá", DayOfWeek.Friday },
                { "So", DayOfWeek.Saturday },
                { "Ne", DayOfWeek.Sunday }
            };

            if (checkBox != null && dayMapping.TryGetValue(checkBox.Content.ToString(), out DayOfWeek day)) {
                if (checkBox.IsChecked == true) {
                    // Přidáme den, pokud není v seznamu
                    if (!selectedDays.Contains(day))
                        selectedDays.Add(day);
                }
                else {
                    // Odebereme den, pokud je v seznamu
                    selectedDays.Remove(day);
                }
            }
            // Aplikovat filtry po změně zaškrtnutí
            ApplyFilters();
        }

        
        private void ModelListBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            selectedModels = ModelListBox.SelectedItems.Cast<string>().ToList();
            ApplyFilters();
        }

        //logika filtrování
        private void ApplyFilters() {
            if (FilteredDataGrid == null)
                return;

            // Filtrovat podle vybraných dnů a modelů
            var filtered = carSalesList
                .Where(car => selectedModels.Contains("Všechny modely") ||
                      selectedModels.Any(model => model.Equals(car.Model, StringComparison.OrdinalIgnoreCase)))
                .Where(car => selectedDays.Contains(car.Date.DayOfWeek));

            // Seskupit podle modelu a spočítat celkovou cenu a počet prodaných kusů
            var summarizedSales = filtered
                .GroupBy(car => car.Model.ToLower())
                .Select(group => new SumarizedCarSales {
                    Model = group.Key,
                    Count = group.Count(),                                   // Počet prodaných kusů
                    TotalPriceWithoutDPH = group.Sum(car => car.Price),      // Celková cena bez DPH
                    TotalPriceWithDPH = group.Sum(car => car.PriceWithDPH)   // Celková cena s DPH
                })
                .ToList();

            // Aktualizovat `FilteredDataGrid` s agregovanými daty
            FilteredDataGrid.ItemsSource = null;
            FilteredDataGrid.ItemsSource = summarizedSales;
        }




    }
}
