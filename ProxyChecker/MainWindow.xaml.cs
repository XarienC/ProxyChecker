using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Leaf.xNet;
using Newtonsoft.Json.Linq;
using System.Threading;

namespace ProxyChecker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public int MaxDegreeOfParallelism { get; set; }

        public MainWindow()
        {
            InitializeComponent();
        }

        private void LoadProxyButton_Click(object sender, RoutedEventArgs e)
        {
            Variables.ProxyList.Clear();
            ProxyLoadedValue.Content = "0";
            Variables.ProgressTarget = 0;
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "Select Proxies";
            openFileDialog.Filter = "Text files (*.txt)|*.txt";
            if (openFileDialog.ShowDialog() == true)
            {
                Variables.ProxyList.AddRange(File.ReadAllLines(openFileDialog.FileName));
                ProxyLoadedValue.Content = Variables.ProxyList.Count;
                CheckingProgress.Maximum = Variables.ProxyList.Count;
                ProgressValue.Content = "0" + "/" + Variables.ProxyList.Count;
                Variables.ProgressTarget = Variables.ProxyList.Count;              
                MessageBox.Show("Imported " + Variables.ProxyList.Count + " proxies.", "Success");

                IEnumerable<string> lines = File.ReadAllLines(openFileDialog.FileName);
                LoadedProxyList.Text = (String.Join(Environment.NewLine, lines));
            }
            else
            {
                MessageBox.Show("Couldn't import proxies, please try again.", "Error");
            }
        }

        private void ClearResultBoxButton_Click(object sender, RoutedEventArgs e)
        {
            ProxyResultList.Items.Clear();
            LoadedProxyList.Text = "";
            ProxyLoadedValue.Content = "0";
            CheckingProgress.Value = 0;
            ProgressValue.Content = "0/0";
            DeadProxiesValue.Content = "0";
            WorkingProxiesValue.Content = "0";
            Variables.ProxyList.Clear();
            Variables.WorkingProxies.Clear();
        }

        private void SaveProxiesButton_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Title = "Save Proxies";
            saveFileDialog.Filter = "Text files (*.txt)|*.txt";
            if (saveFileDialog.ShowDialog() == true)
            {
                using (var file = new StreamWriter(saveFileDialog.FileName))
                {
                    Variables.WorkingProxies.ForEach(proxy => file.WriteLine(proxy));
                }
                MessageBox.Show("Saved " + Variables.WorkingProxies.Count() + " proxies.", "Success");
            }
            else
            {
                MessageBox.Show("Error while saving proxies.", "Error");
            }
        }

        private void httpsradiobutton_Checked(object sender, RoutedEventArgs e)
        {
            Variables.ProxyType = "https";
        }

        private void socks4radiobutton_Checked(object sender, RoutedEventArgs e)
        {
            Variables.ProxyType = "socks4";
        }

        private void socks5radiobutton_Checked(object sender, RoutedEventArgs e)
        {
            Variables.ProxyType = "socks5";
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            Variables.WorkingProxies.Clear();
            CheckingProgress.Value = 0;
            DeadProxiesValue.Content = 0;
            WorkingProxiesValue.Content = 0;
            ProgressValue.Content = "0/";
            Variables.GoodProxies = 0;
            Variables.BadProxies = 0;
            Variables.Progress = 0;

            ClearResultBoxButton.IsEnabled = false;
            LoadProxyButton.IsEnabled = false;
            StartButton.IsEnabled = false;
            SaveProxiesButton.IsEnabled = false;
            httpsradiobutton.IsEnabled = false;
            socks4radiobutton.IsEnabled = false;
            socks5radiobutton.IsEnabled = false;

            new Thread(() =>
            {
                object sync = new Object();
                Parallel.ForEach(Variables.ProxyList, new ParallelOptions
                {
                    MaxDegreeOfParallelism = Variables.MaxThreads
                },
                (Proxy, _, lineNumber) =>
                {
                    try
                    {
                        HttpRequest request = new HttpRequest();
                        request.IgnoreProtocolErrors = true;
                        request.ConnectTimeout = Variables.ProxyTimeout;
                        if (Variables.ProxyType.ToLower() == "https")
                        {
                            request.Proxy = HttpProxyClient.Parse(Proxy);
                        }
                        if (Variables.ProxyType.ToLower() == "socks4")
                        {
                            request.Proxy = Socks4ProxyClient.Parse(Proxy);
                        }
                        if (Variables.ProxyType.ToLower() == "socks5")
                        {
                            request.Proxy = Socks5ProxyClient.Parse(Proxy);
                        }
                        var proxyinfo = request.Get("http://ip-api.com/json/");
                        string code = proxyinfo.StatusCode.ToString().ToLower();
                        JObject jobj = JObject.Parse(proxyinfo.ToString());
                        string Country = (jobj.SelectToken("country").ToString());
                        string isp = (jobj.SelectToken("isp").ToString());
                        string status = (jobj.SelectToken("status").ToString().ToLower());
                        if (status == "success" && code == "200" || code == "ok")
                        {
                            Dispatcher.Invoke(() =>
                            {
                                Interlocked.Increment(ref Variables.GoodProxies);
                                WorkingProxiesValue.Content = Variables.GoodProxies.ToString();
                                Variables.WorkingProxies.Add(Proxy);
                                ProxyResultList.Items.Add(Proxy + " | " + Country + " | " + isp);
                            });
                        }
                        else
                        {
                            Dispatcher.Invoke(() =>
                            {
                                Interlocked.Increment(ref Variables.BadProxies);
                                DeadProxiesValue.Content = Variables.BadProxies.ToString();
                            });
                        }
                    }
                    catch (Exception)
                    {
                        Dispatcher.Invoke(() =>
                        {
                            Interlocked.Increment(ref Variables.BadProxies);
                            DeadProxiesValue.Content = Variables.BadProxies.ToString();
                        });
                    }
                    Dispatcher.Invoke(() =>
                    {
                        Interlocked.Increment(ref Variables.Progress);
                        ProgressValue.Content = Variables.Progress.ToString() + "/" + Variables.ProgressTarget;
                        CheckingProgress.Value = CheckingProgress.Value + 1;
                    });
                });
                Dispatcher.Invoke(() =>
                {
                    ClearResultBoxButton.IsEnabled = true;
                    LoadProxyButton.IsEnabled = true;
                    StartButton.IsEnabled = true;
                    SaveProxiesButton.IsEnabled = true;
                    httpsradiobutton.IsEnabled = true;
                    socks4radiobutton.IsEnabled = true;
                    socks5radiobutton.IsEnabled = true;
                });
            }).Start();
        }
    }
}
