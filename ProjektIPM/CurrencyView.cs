using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjektIPM
{
    public class CurrencyView : INotifyPropertyChanged
    {
        public string Name { get; set; }
        public string CrCode { get; set; }
        public double Mid { get; set; }

        public int MidExchanger { get; set; }

        public CurrencyView() { }
        public CurrencyView(string CrCode, string Name, double Mid, int MidExchanger)
        {
            this.Name = Name;
            this.CrCode = CrCode;
            this.Mid = Mid;
            this.MidExchanger = MidExchanger;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public static string CheckName(string code)
        {
            Dictionary<string, string> appropriateName = new Dictionary<string, string>();
            appropriateName.Add("THB", "bat (Tajlandia)");
            appropriateName.Add("USD", "dolar amerykański");
            appropriateName.Add("AUD", "dolar australijski");
            appropriateName.Add("HKD", "dolar Hongkongu");
            appropriateName.Add("CAD", "dolar kanadyjski");
            appropriateName.Add("NZD", "dolar nowozelandzki");
            appropriateName.Add("SGD", "dolar singapurski");
            appropriateName.Add("EUR", "euro");
            appropriateName.Add("HUF", "forint (Węgry)");
            appropriateName.Add("CHF", "frank szwajcarski");
            appropriateName.Add("GBP", "funt szterling");
            appropriateName.Add("UAH", "hrywna (Ukraina)");
            appropriateName.Add("JPY", "jen (Japonia)");
            appropriateName.Add("CZK", "korona czeska");
            appropriateName.Add("DKK", "korona duńska");
            appropriateName.Add("ISK", "korona islandzka");
            appropriateName.Add("NOK", "korona norweska");
            appropriateName.Add("SEK", "korona szwedzka");
            appropriateName.Add("HRK", "kuna (Chorwacja)");
            appropriateName.Add("RON", "lej rumuński");
            appropriateName.Add("BGN", "lew (Bułgaria)");
            appropriateName.Add("TRY", "lira turecka");
            appropriateName.Add("ILS", "nowy izraelski szekel");
            appropriateName.Add("CLP", "peso chilijskie");
            appropriateName.Add("PHP", "peso filipińskie");
            appropriateName.Add("MXN", "peso meksykańskie");
            appropriateName.Add("ZAR", "rand (Republika Południowej Afryki)");
            appropriateName.Add("BRL", "real (Brazylia)");
            appropriateName.Add("MYR", "ringgit (Malezja)");
            appropriateName.Add("RUB", "rubel rosyjski");
            appropriateName.Add("IDR", "rupia indonezyjska");
            appropriateName.Add("INR", "rupia indyjska");
            appropriateName.Add("KRW", "won południowokoreański");
            appropriateName.Add("CNY", "yuan renminbi (Chiny)");
            appropriateName.Add("XDR", "SDR (MFW)");

            return appropriateName[code];
        }


        public override string ToString()
        {
            return this.Name + "\n" + this.Mid + " Przelicznik: " + this.MidExchanger;
        }
    }
}

