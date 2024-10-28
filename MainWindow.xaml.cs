using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;
using Task.Model;
using Task.View;

namespace Task {

    public partial class MainWindow : Window {
        //deklarace kolekcí pro uchovávání objektů určitých typů
        private List<CarSales> carSalesList = new List<CarSales>();
        private List<string> selectedModels = new List<string>();
        private List<DayOfWeek> selectedDays = new List<DayOfWeek> { DayOfWeek.Saturday, DayOfWeek.Sunday };

        public MainWindow() {
            InitializeComponent();
            SalesDataGrid.ItemsSource = carSalesList;
        }

        //metoda pro přidávání modelu do seznamu ručně
        private void BtnAddItem_Click(object sender, RoutedEventArgs e) {
            var addItemWindow = new AddItemWindow();
            Opacity = 0.4;
            addItemWindow.ShowDialog();
            Opacity = 1;
            //pokud vytvoření nového záznamu proběhlo úspěšně, je přidán do seznamu a proběhne aktualizace seznamu a filtrovaného seznamu
            if (addItemWindow.IsItemAdded && addItemWindow.NewCarSale != null) {
                carSalesList.Add(addItemWindow.NewCarSale);
                RefreshSalesDataGrid();
                UpdateModelListBox();
            }
        }

        //import xml souboru
        private void BtnImportFile_Click(object sender, RoutedEventArgs e) {
            Opacity = 0.4;
            OpenFileDialog ofd = new OpenFileDialog { Filter = "Vložení XML souboru | *.xml", Title = "Vložte, prosím, soubor XML.", Multiselect = false };
            if (ofd.ShowDialog() == true) {
                carSalesList.AddRange(LoadDataFromXml(ofd.FileName));
                RefreshSalesDataGrid();
                UpdateModelListBox();
            }
            Opacity = 1;           
        }

        //metoda pro parsování dat z xml na objekty typu CarSales, pomocí LINQ to xml
        private List<CarSales> LoadDataFromXml(string filePath) {
            XDocument xdoc = XDocument.Load(filePath);
            return xdoc.Descendants("CarSale")
                .Select(x => new CarSales {
                    Model = x.Element("Model")?.Value ?? "Neznámý model",
                    Date = DateTime.TryParse(x.Element("Date")?.Value, out var date) ? date : DateTime.Now,
                    Price = double.TryParse(x.Element("Price")?.Value, out var price) ? price : 0.0,
                    DPH = double.TryParse(x.Element("DPH")?.Value, out var dph) ? dph : 0.0
                })
                .ToList();
        }

        //pro aktualizaci seznamu, volána v metodách pro přidávání, importu a mazání záznamů
        private void RefreshSalesDataGrid() {
            SalesDataGrid.ItemsSource = null;
            SalesDataGrid.ItemsSource = carSalesList; 
        }

        //metoda pro mazání záznamů v seznamu
        private void BtnDelete_Click(object sender, RoutedEventArgs e) {
            var selectedItems = SalesDataGrid.SelectedItems.Cast<CarSales>().ToList();
            if (selectedItems.Count > 0) {
                var result = MessageBox.Show("Opravdu chcete smazat vybrané položky?", "Potvrzení smazání", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes) {
                    selectedItems.ForEach(item => carSalesList.Remove(item));
                    RefreshSalesDataGrid();
                    ApplyFilters();
                    UpdateModelListBox();
                }
            }
            else {
                MessageBox.Show("Žádné položky nejsou vybrány.", "Chyba", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        //pro aktualizaci položek v ListBoxu na základě unikátních názvů modelů, zárověň nastavuje case insensitive
        private void UpdateModelListBox() {
            var modelNames = carSalesList
                .GroupBy(car => car.Model.ToLower())
                .Select(group => group.First().Model)
                .ToList();
            modelNames.Insert(0, "Všechny modely");
            ModelListBox.ItemsSource = modelNames;
            ModelListBox.SelectedItems.Clear();
            ModelListBox.SelectedItems.Add("Všechny modely");
        }

        //metoda pro přidávání nebo odebírání dny do/z selectedDays na základě zaškrtnutí nebo odškrtnutí CheckBoxů
        //také mapování ze systémových názvů (DayOfWeek) na české
        private void DayCheckBox_Changed(object sender, RoutedEventArgs e) {
            var checkBox = sender as CheckBox;
            var dayMapping = new Dictionary<string, DayOfWeek> {
            { "Po", DayOfWeek.Monday }, { "Út", DayOfWeek.Tuesday }, { "St", DayOfWeek.Wednesday },
            { "Čt", DayOfWeek.Thursday }, { "Pá", DayOfWeek.Friday }, { "So", DayOfWeek.Saturday }, { "Ne", DayOfWeek.Sunday }
        };

            if (checkBox != null && dayMapping.TryGetValue(checkBox.Content.ToString(), out var day)) {
                if (checkBox.IsChecked == true) {
                    if (!selectedDays.Contains(day)) selectedDays.Add(day);
                }
                else {
                    selectedDays.Remove(day);
                }
            }
            ApplyFilters();
        }

        //metoda pro dynamické filtrování záznamů na základě toho, jaké modely jsou vybrány v ListBoxu 
        private void ModelListBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            selectedModels = ModelListBox.SelectedItems.Cast<string>().ToList();
            ApplyFilters();
        }

        //metoda aktualizuje fitrovaný seznam
        private void ApplyFilters() {           
            if (FilteredDataGrid == null) return; //pojistka po spuštění appky, pokud neexistují data k filtrování
            UpdateTotalCounts();

            var filtered = FilterBySelectedDays(FilterBySelectedModels(carSalesList)); //dvojí filtrování
            //agregace dat
            var summarizedSales = filtered 
                .GroupBy(car => car.Model.ToLower())
                .Select(group => new SumarizedCarSales {
                    Model = group.Key,
                    Count = group.Count(),
                    TotalPriceWithoutDPH = group.Sum(car => car.Price),
                    TotalPriceWithDPH = group.Sum(car => car.PriceWithDPH)
                })
                .ToList();
            //aktualizace seznamu filtrovaných objektů
            FilteredDataGrid.ItemsSource = null;
            FilteredDataGrid.ItemsSource = summarizedSales;
        }

        //filtrovací metoda dle všech modelů, nebo dle vybraného modelu/modelů (ListBox)
        private IEnumerable<CarSales> FilterBySelectedModels(IEnumerable<CarSales> cars) {
            return cars.Where(car => selectedModels.Contains("Všechny modely") ||
                                     selectedModels.Any(model => model.Equals(car.Model, StringComparison.OrdinalIgnoreCase)));
        }

        //filtrovací metoda dle zvolených dnů (checkboxy)
        private IEnumerable<CarSales> FilterBySelectedDays(IEnumerable<CarSales> cars) {
            return cars.Where(car => selectedDays.Contains(car.Date.DayOfWeek));
        }

        //aktualizace dodatečných informací
        private void UpdateTotalCounts() {
            TotalCarsCount.Text = carSalesList.Count.ToString();
            TotalModelCount.Text = carSalesList.Select(car => car.Model.ToLower()).Distinct().Count().ToString();
            TotalPriceWithoutDPH.Text = carSalesList.Sum(car => car.Price).ToString("N2");
            TotalPriceWithDPH.Text = carSalesList.Sum(car => car.PriceWithDPH).ToString("N2");
        }
    }
}
