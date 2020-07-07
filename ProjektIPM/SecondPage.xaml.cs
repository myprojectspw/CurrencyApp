using Microsoft.Toolkit.Uwp.UI.Controls;
using Newtonsoft.Json;
using SharpDX.WIC;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using WinRTXamlToolkit.Controls.DataVisualization.Charting;
using Windows.Graphics.Imaging;
using Windows.Graphics.Display;
using System.Threading.Tasks;

namespace ProjektIPM
{
    public sealed partial class SecondPage : Page
    {
        private string currencyMain;
        public ViewModelSecondPage viewModel2 = new ViewModelSecondPage();
        private const int FULL_PROGRESS = 20;
        private string responseBody = "";

        public SecondPage()
        {
            this.InitializeComponent();
            LoadPreviousCurrenciesToListForChart();
        }

        //Inizialize functions

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is string && !string.IsNullOrWhiteSpace((string)e.Parameter))
            {
                currencyMain = e.Parameter.ToString();
                currentCurrency.Text = $"Historia waluty: {e.Parameter.ToString()} /PLN";
            }
            else
            {
                currentCurrency.Text = "Historia waluty: /PLN";
            }
            base.OnNavigatedTo(e);
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

            dt.Columns.Add("Dzien", typeof(string));
            dt.Columns.Add("Kurs", typeof(string));
            //items = new List<CurrencyView>();
            for (int i = 0; i < this.viewModel2.ItemsChart.Count; i++)
            {
                Rate r = this.viewModel2.ItemsChart[i];
                dt.Rows.Add(r.effectiveDate.ToString("yyyy-MM-dd"), r.mid);
            }
            return dt;
        }

        //Control functions

        private void DownloadDataFromIntervalInTextBoxes(object sender, RoutedEventArgs e)
        {
            DownloadData();
        }

        private void InicializeProgressBar()
        {
            this.viewModel2.Start = 0;
            this.viewModel2.Progress = 0;
            this.viewModel2.Status = "Pobieranie...";
        }

        private void EndStatusProgressBar()
        {
            this.viewModel2.Status = "Pobrano";
            this.viewModel2.Progress = FULL_PROGRESS;
        }

        public async void DownloadData()
        {
            this.viewModel2.ItemsChart = new List<Rate>();
            System.Diagnostics.Debug.WriteLine(this.viewModel2.FirstDate + " | " + this.viewModel2.SecondDate);
            try
            {
                string[] pom = this.viewModel2.FirstDate.Split("-");
                DateTime a = new DateTime(Convert.ToInt32(pom[0]), Convert.ToInt32(pom[1]), Convert.ToInt32(pom[2]));

                pom = this.viewModel2.SecondDate.Split("-");
                DateTime b = new DateTime(Convert.ToInt32(pom[0]), Convert.ToInt32(pom[1]), Convert.ToInt32(pom[2]));

                TimeSpan s = b.Subtract(a);
                int time = s.Days;
                int interval = s.Days / 200;
                int counter = -200;
                DateTime actual = a;
                DateTime date = b;
                DateTime lastDate = a;
                List<int> values = new List<int>();
                bool status = true;

                if (time < 0) status = false;
                if (a < new DateTime(2002, 2, 1))
                {
                    this.viewModel2.Status = "Brak danych dla waluty";
                    status = false;
                }

                
                if (status)
                {
                    InicializeProgressBar();
                    if (Math.Abs(time) < 200)
                    {
                        await DownloadDataFromTableANBP(actual, date);

                        EndStatusProgressBar();
                    }
                    else
                    {
                        int counterTime = this.viewModel2.Finish / interval;
                        actual = b.AddDays(counter);
                        date = b;

                        do
                        {
                            //System.Diagnostics.Debug.WriteLine(counter + " | " + actual + " | " + date);
                            if (actual < lastDate)
                            {
                                time = b.Subtract(a).Days;
                                actual = b.AddDays(-time);
                                //System.Diagnostics.Debug.WriteLine(counter + " | " + actual + " | " + date);
                            }

                            await DownloadDataFromTableANBP(actual, date);

                            this.viewModel2.Progress += counterTime;

                            System.Threading.Thread.Sleep(500);

                            date = date.AddDays(counter);
                            actual = actual.AddDays(counter);

                            if (actual < lastDate)
                            {
                                time = date.Subtract(lastDate).Days;
                                actual = date.AddDays(-time);

                                await DownloadDataFromTableANBP(actual, date);
                                break;
                            }
                        } while (date > lastDate);

                        EndStatusProgressBar();
                    }
                }

                this.viewModel2.ItemsChart = this.viewModel2.ItemsChart.OrderBy(x => x.effectiveDate.Date).ToList();

                DataTable dt = GetDataTable();
                FillDataGrid(dt, dataGridCurrency);
                dataGridCurrency.ItemsSource = dt.DefaultView;

                GetChartData(this.viewModel2.ItemsChart);

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                this.viewModel2.Status = "Blad";
            }
        }

        private async Task DownloadDataFromTableANBP(DateTime actual, DateTime date)
        {
            try
            {
                HttpClient client = new HttpClient();
                string urlEuro = "https://api.nbp.pl/api/exchangerates/rates/a/" + currencyMain + "/" + actual.ToString("yyyy-MM-dd") + "/" + date.ToString("yyyy-MM-dd") + "/";
                HttpResponseMessage response = await client.GetAsync(urlEuro);
                response.EnsureSuccessStatusCode();
                this.responseBody = await response.Content.ReadAsStringAsync();

                InsertDataToItemsChartList(this.responseBody);
            }
            catch(Exception e)
            {
                this.viewModel2.Status = "Brak Danych dla waluty";
                System.Diagnostics.Debug.WriteLine(e.Message);
            }

        }


        private void InsertDataToItemsChartList(string responseBody)
        {
            Currency c = JsonConvert.DeserializeObject<Currency>(responseBody);
            foreach (Rate r in c.rates)
            {
                this.viewModel2.ItemsChart.Add(new Rate(r.mid, r.effectiveDate));
            }
        }

        public void GetChartData(List<Rate> rate)
        {
            List<ChartData> lista = new List<ChartData>();
            foreach(Rate r in rate)
            {
                lista.Add(new ChartData(r.effectiveDate, r.mid));
            }
            (LineChart.Series[0] as LineSeries).ItemsSource = lista;
        }

        public async void LoadPreviousCurrenciesToListForChart()
        {
            var list = new List<Rate>();
            try
            {
                Windows.Storage.StorageFolder storageFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
                Windows.Storage.StorageFile sampleFile = await storageFolder.GetFileAsync("sampleChart.json");
                var text = await FileIO.ReadTextAsync(sampleFile);
                list = JsonConvert.DeserializeObject<List<Rate>>(text);
                this.viewModel2.ItemsChart = list;

                //Load list to datatable and chart
                DataTable dt = GetDataTable();
                FillDataGrid(dt, this.dataGridCurrency);
                dataGridCurrency.ItemsSource = dt.DefaultView;
                GetChartData(this.viewModel2.ItemsChart);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }

        private async void SaveChartToPngFile(object sender, RoutedEventArgs e)
        {
            RenderTargetBitmap rtb = new RenderTargetBitmap();
            await rtb.RenderAsync(LineChart);

            var pixelBuffer = await rtb.GetPixelsAsync();
            var pixels = pixelBuffer.ToArray();
            var displayInformation = DisplayInformation.GetForCurrentView();

            var savePicker = new Windows.Storage.Pickers.FileSavePicker();
            savePicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary;
            savePicker.FileTypeChoices.Add("PNG Bitmap", new List<string>() { ".png" });
            Windows.Storage.StorageFile file = await savePicker.PickSaveFileAsync();
            if (file != null)
            {
                Windows.Storage.CachedFileManager.DeferUpdates(file);
                await Windows.Storage.FileIO.WriteTextAsync(file, file.Name);
                Windows.Storage.Provider.FileUpdateStatus status = await Windows.Storage.CachedFileManager.CompleteUpdatesAsync(file);

                if (status == Windows.Storage.Provider.FileUpdateStatus.Complete)
                {
                    using (var stream = await file.OpenAsync(FileAccessMode.ReadWrite))
                    {
                        var encoder = await Windows.Graphics.Imaging.BitmapEncoder.CreateAsync(Windows.Graphics.Imaging.BitmapEncoder.PngEncoderId, stream);
                        encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied, (uint)rtb.PixelWidth, (uint)rtb.PixelHeight, displayInformation.RawDpiX, displayInformation.RawDpiY, pixels);
                        await encoder.FlushAsync();
                    }
                    this.viewModel2.Status = "Wykres zapisano";
                }
                else
                {
                    this.viewModel2.Status = "Wykres niezapisano";
                }
            }
            else
            {
                return;
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(MainPage));
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            CoreApplication.Exit();
        }
    }
}
