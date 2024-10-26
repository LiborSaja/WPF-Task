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
        private List<CarSales> carSalesList = new List<CarSales>();
        public MainWindow() {
            DataContext = this;
            InitializeComponent();

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
                }
            }
            else {
                MessageBox.Show("Žádné položky nejsou vybrány.", "Chyba", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        //ListBox pro výběr modelu
        private void UpdateModelListBox() {
            // Skupina modelů s počty
            var modelCounts = carSalesList
                .GroupBy(car => car.Model)
                .Select(g => new ModelCount { ModelName = g.Key, Count = g.Count() })
                .ToList();

            // Přidáme položku "Všechny modely" na začátek
            modelCounts.Insert(0, new ModelCount { ModelName = "Všechny modely", Count = carSalesList.Count });

            // Nastavení datového zdroje pro ListBox
            ModelListBox.ItemsSource = modelCounts;
        }


    }
}
