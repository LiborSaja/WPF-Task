
namespace Task.Model {
    internal class SumarizedCarSales {
        public string Model { get; set; }
        public int Count { get; set; }
        public double TotalPriceWithoutDPH { get; set; }
        public double TotalPriceWithDPH { get; set; }

        public override string ToString() {
            return Model;
        }
    }
}
