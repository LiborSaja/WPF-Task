using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Task.Model {
    internal class ModelCount {
        public string ModelName { get; set; }
        public int Count { get; set; }

        public override string ToString() {
            return $"{ModelName} ({Count})";
        }
    }
}
