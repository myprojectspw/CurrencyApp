using System;
using System.Collections.Generic;
using System.Net;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.Net.Http;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using System.Data;
using System.Collections.ObjectModel;
using Microsoft.Toolkit.Uwp.UI.Controls;
using Windows.ApplicationModel.Core;
using Windows.Storage;
namespace ProjektIPM
{
    public sealed partial class MainPage : Page
    {
        List<String> itemsData = new List<String>();
        List<Currency> tableA;
        List<CurrencyTable> tableB;
        ViewModel ViewModel = new ViewModel();

        public MainPage()
        {
            this.InitializeComponent();
            try
            {
                InicializeDataList();
                CreateCurrencies();
                LoadPreviousCurrenciesToTable();
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.ToString());
            }
        }

        //Inicialize functions

        private void InicializeDataList()
        {
            string todayD = DateTime.Today.ToString("yyyy-MM-dd");
            TimeSpan span = DateTime.Today.Subtract(new DateTime(2002, 2, 01));
            int time = span.Days;
            for (int d = time; d >= 0; d--)
            {
                string ago = (DateTime.Today.AddDays(-d)).ToString("yyyy-MM-dd");
                itemsData.Add(ago);
            }
            Datas.ItemsSource = itemsData;
        }

        private async Task CreateCurrencies()
        {
            tableA = new List<Currency>();
            HttpClient client = new HttpClient();
            string url = "https://api.nbp.pl/api/exchangerates/tables/A/";

            try
            {
                HttpResponseMessage response2 = await client.GetAsync(url);
                response2.EnsureSuccessStatusCode();
                string responseBody2 = await response2.Content.ReadAsStringAsync();

                System.Diagnostics.Debug.WriteLine(responseBody2);
                tableB = new List<CurrencyTable>();
                tableB = JsonConvert.DeserializeObject<List<CurrencyTable>>(responseBody2);

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }

        private async Task GetCurrenciesFromDate()
        {
            String text = Datas.SelectedItems[0].ToString();
            HttpClient client = new HttpClient();
            bool exist = false;
            tableA = new List<Currency>();
            this.ViewModel.Items = new List<CurrencyView>();

            string responseBody = "";
            string[] pom = text.Split('-');
            DateTime a = new DateTime(Convert.ToInt32(pom[0]), Convert.ToInt32(pom[1]), Convert.ToInt32(pom[2]));
            int i = 0;

            foreach (CurrencyTable cr in tableB)
            {
                foreach (TableRate r in cr.rates)
                {
                    try
                    {
                        string urlEuro = "https://api.nbp.pl/api/exchangerates/rates/a/" + r.code.ToLower() + "/" + a.ToString("yyyy-MM-dd") + "/";
                        HttpResponseMessage response = await client.GetAsync(urlEuro);
                        response.EnsureSuccessStatusCode();
                        responseBody = await response.Content.ReadAsStringAsync();
                        tableA.Add(new Currency());
                        tableA[i] = JsonConvert.DeserializeObject<Currency>(responseBody);
                        i++;
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine(ex.Message);
                    }
                }
            }

            foreach (Currency cr in tableA)
            {
                foreach (Rate c in cr.rates)
                {
                    System.Diagnostics.Debug.WriteLine(cr.currency);
                    if (c.effectiveDate.ToString("yyyy-MM-dd").Equals(text))
                    {
                        if (c.mid > 1) this.ViewModel.Items.Add(new CurrencyView(cr.code, cr.currency, c.mid, 1));
                        else if (c.mid < 1 && c.mid > 0.1) this.ViewModel.Items.Add(new CurrencyView(cr.code, cr.currency, c.mid * 10, 10));
                        else if (c.mid < 0.1 && c.mid > 0.01) this.ViewModel.Items.Add(new CurrencyView(cr.code, cr.currency, c.mid * 100, 100));
                        else if (c.mid < 0.01 && c.mid > 0.001) this.ViewModel.Items.Add(new CurrencyView(cr.code, cr.currency, c.mid * 1000, 1000));
                        else this.ViewModel.Items.Add(new CurrencyView(cr.code, cr.currency, c.mid * 10000, 10000));
                    }
                }
            }

            DataTable dt = GetDataTable();
            FillDataGrid(dt, dataGrid);
            dataGrid.ItemsSource = dt.DefaultView;

            foreach (Currency cr in tableA)
            {
                foreach (Rate c in cr.rates)
                {
                    if (c.effectiveDate.ToString("yyyy-MM-dd").Equals(text))
                    {
                        exist = true;
                    }
                }
            }
            if (!exist) this.ViewModel.Exist = "Brak Danych";
            else this.ViewModel.Exist = "";
        }

        public static void FillDataGrid(DataTable table, DataGrid grid)
        {
            grid.Columns.Clear();
            grid.AutoGenerateColumns = false;
            for (int i = 0; i < table.Columns.Count; i++)
            {
                grid.Columns.Add(new DataGridTextColumn()
                {
                    Header = table.Columns[i].ColumnName,
                    Binding = new Binding { Path = new PropertyPath("[" + i.ToString() + "]") }
                });
            }

            var collection = new ObservableCollection<object>();
            foreach (DataRow row in table.Rows)
            {
                collection.Add(row.ItemArray);
            }
            grid.ItemsSource = collection;
        }

        private DataTable GetDataTable()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Dzien", typeof(int));
            dt.Columns.Add("Nazwa Waluty", typeof(string));
            dt.Columns.Add("Skrot", typeof(string));
            dt.Columns.Add("Kurs", typeof(string));
            for (int i = 0; i < this.ViewModel.Items.Count; i++)
            {
                CurrencyView c = this.ViewModel.Items[i];
                if (c.Name == null) dt.Rows.Add(i + 1, CurrencyView.CheckName(c.CrCode), c.CrCode, c.Mid);
                else dt.Rows.Add(i + 1, c.Name, c.CrCode, c.Mid);
            }
            return dt;
        }

        public async void LoadPreviousCurrenciesToTable()
        {
            var list = new List<CurrencyView>();
            try
            {
                Windows.Storage.StorageFolder storageFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
                Windows.Storage.StorageFile sampleFile = await storageFolder.GetFileAsync("sample.json");
                var text = await FileIO.ReadTextAsync(sampleFile);
                list = JsonConvert.DeserializeObject<List<CurrencyView>>(text);
                this.ViewModel.Items = list;

                DataTable dt = GetDataTable();
                FillDataGrid(dt, this.dataGrid);
                dataGrid.ItemsSource = dt.DefaultView;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }

        //Controls Functions

        private void ChangePageOnClickCurrency(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                DataRowView currency = (DataRowView)dataGrid.SelectedItem;
                Object[] c = currency.Row.ItemArray;
                this.Frame.Navigate(typeof(SecondPage), c[2].ToString());
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }

        private async void DownloadCurrenciesOnChangeDateTimeList(object sender, SelectionChangedEventArgs e)
        {
            this.ViewModel.DateOfPublication = (string)Datas.SelectedItem;
            await GetCurrenciesFromDate();
            if (this.ViewModel.Items.Count == 0) this.ViewModel.Exist = "Brak Danych";
            else this.ViewModel.Exist = "";
        }

        private void ExitButtonFromApp(object sender, RoutedEventArgs e)
        {
            CoreApplication.Exit();
        }
    }
}

