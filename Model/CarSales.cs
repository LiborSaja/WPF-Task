using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Task.Model {
    public class CarSales {
        public int Id { get; set; } 
        public string Model { get; set; }
        public DateTime Date { get; set; }
        public double Price { get; set; }
        public double DPH { get; set; }
    }
}
