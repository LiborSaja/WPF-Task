using System;
using System.Windows;
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
                // získání a ověření hodnot z textových polí
                double price = double.Parse(PriceWithoutDPHTextBox.Text);
                double dph = double.Parse(DPHTextBox.Text);
                if (price < 0 || dph < 0) {
                    MessageBox.Show("Zadejte kladné hodnoty pro cenu a DPH.", "Chyba", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // vytvoření nového objektu CarSales s hodnotami zadanými uživatelem
                NewCarSale = new CarSales {
                    Model = ModelTextBox.Text,
                    Date = SaleDatePicker.SelectedDate ?? DateTime.Now,
                    Price = double.Parse(PriceWithoutDPHTextBox.Text),
                    DPH = double.Parse(DPHTextBox.Text)
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
