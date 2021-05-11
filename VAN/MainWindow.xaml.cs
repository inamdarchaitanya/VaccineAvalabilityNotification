﻿using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using VAN.Model;

namespace VAN
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        DispatcherTimer dispatcherTimer;


        public MainWindow()
        {
            InitializeComponent();
            //  DispatcherTimer setup
            dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 10);

        }

        [DllImport("kernel32.dll", EntryPoint = "Beep", SetLastError = true, ExactSpelling = true)]
        public static extern bool Beep(uint frequency, uint duration);

       

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var client = new RestClient("https://cdn-api.co-vin.in/api/v2/admin/location/states");
            client.Timeout = -1;
            var request = new RestRequest(Method.GET);
            client.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/90.0.4430.93 Safari/537.36";
            IRestResponse response = client.Execute(request);
            Console.WriteLine(response.Content);

            RootState x = JsonConvert.DeserializeObject<RootState>(response.Content);

            foreach (var state in x.States)
            {
                ComboBoxItem item = new ComboBoxItem();
                item.Content = state.state_name;
                item.Uid = state.state_id.ToString();
                stCombo.Items.Add(item);
            }
        }

        private void stCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            disCombo.Items.Clear();

            string stateId = ((System.Windows.UIElement)stCombo.SelectedItem).Uid;


            var client = new RestClient("https://cdn-api.co-vin.in/api/v2/admin/location/districts/" + stateId);
            client.Timeout = -1;
            var request = new RestRequest(Method.GET);
            client.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/90.0.4430.93 Safari/537.36";
            IRestResponse response = client.Execute(request);
            Console.WriteLine(response.Content);

            RootDistrict x = JsonConvert.DeserializeObject<RootDistrict>(response.Content);

            foreach (var dis in x.Districts)
            {
                ComboBoxItem item = new ComboBoxItem();
                item.Content = dis.district_name;
                item.Uid = dis.district_id.ToString();
                disCombo.Items.Add(item);
            }
        }


        void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            dg18.ItemsSource = null;
            dg45.ItemsSource = null;
            dg18.Items.Clear();
            dg45.Items.Clear();
            DateTime dtCurrent = DateTime.Now;
            string text = string.Empty;

            _18ind.Fill = new SolidColorBrush(Colors.Red);
            _45ind.Fill = new SolidColorBrush(Colors.Red);

            Dictionary<DateTime, string> centers = new Dictionary<DateTime, string>();
            List<Session> listSession18 = new List<Session>();
            List<Session> listSession45 = new List<Session>();
            for (int i = 0; i < 2; i++)
            {
                dtCurrent = dtCurrent.AddDays(i);

                var client = new RestClient("https://cdn-api.co-vin.in/api/v2/appointment/sessions/public/findByDistrict?district_id=" + ((System.Windows.UIElement)disCombo.SelectedItem).Uid + "&date=" + dtCurrent.ToString("dd-MM-yyyy"));
                client.Timeout = -1;
                var request = new RestRequest(Method.GET);
                client.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/90.0.4430.93 Safari/537.36";
                IRestResponse response = client.Execute(request);
                Console.WriteLine(response.Content);

                text += response.Content;
                RootSession x = JsonConvert.DeserializeObject<RootSession>(response.Content);
                foreach (var session in x.Sessions)
                {
                    if (session.min_age_limit == 18)
                    {
                        listSession18.Add(session);
                    }
                    else
                    {
                        listSession45.Add(session);
                    }
                }
            }

            if (listSession18.Count > 0)
            {
                dg18.ItemsSource = listSession18;
                _18ind.Fill = new SolidColorBrush(Colors.ForestGreen);
                Beep(5000, 1000);
                Thread.Sleep(100);
                Beep(5000, 1000);
                Thread.Sleep(100);
                Beep(5000, 1000);
                Thread.Sleep(100);
            }
           
            if (listSession45.Count > 0)
            {
                dg45.ItemsSource = listSession45;

                _45ind.Fill = new SolidColorBrush(Colors.ForestGreen);

                if (text.Contains("Only Armed"))
                {
                    _45ind.Fill = new SolidColorBrush(Colors.Orange);                    
                }
            }
            if (text.StartsWith("<!DOCTYPE"))
            {
                //Error too many requests sent.
                Beep(2500, 3000);
                Thread.Sleep(100);
            }

            Beep(1000, 500);
            Thread.Sleep(50);

        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if ((bool)MonitorChkBox.IsChecked)
            {
                dispatcherTimer.Start();
            }
            else
            {
                dispatcherTimer.Stop();
            }
        }
    }
}
