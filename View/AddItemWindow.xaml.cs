using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;
using Task.Model;

namespace Task.View {
    
    public partial class AddItemWindow : Window {
        public CarSales NewCarSale { get; private set; }
        public bool IsItemAdded { get; private set; } = false;
        public AddItemWindow() {
            InitializeComponent();
        }        

        private void BtnAdd_Click(object sender, RoutedEventArgs e) {
            try {
                // Vytvoříme novou položku typu CarSales a nastavíme její vlastnosti z formuláře
                NewCarSale = new CarSales {
                    Model = ModelTextBox.Text,
                    Date = SaleDatePicker.SelectedDate ?? DateTime.Now,
                    Price = double.Parse(PriceWithoutVATTextBox.Text),
                    DPH = double.Parse(VATTextBox.Text)
                };

                IsItemAdded = true;                
                this.Close();
            }
            catch (FormatException) {
                MessageBox.Show("Zadejte platné hodnoty pro všechny položky.", "Chyba", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
