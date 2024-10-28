using System;

namespace Task.Model {
    public class CarSales {
        public string Model { get; set; }
        public DateTime Date { get; set; }
        public double Price { get; set; }
        public double DPH { get; set; }
        public double PriceWithDPH => Price + ((Price * DPH) / 100);//výpočet dph
        public string DayOfWeekShort => Date.ToString("ddd", new System.Globalization.CultureInfo("cs-CZ"));//zjištění dne z data
    }
}
