using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjektIPM
{
    public class ChartData
    {
        public DateTime dateC { get; set; }
        public double valueC { get; set; }

        public ChartData(DateTime dateC, double valueC)
        {
            this.dateC = dateC;
            this.valueC = valueC;
        }

        
    }
}