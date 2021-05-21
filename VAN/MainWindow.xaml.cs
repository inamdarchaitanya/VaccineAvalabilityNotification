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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using VAN.Common;
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
            dgArmy.ItemsSource = null;
            dgArmy.Items.Clear();
            dg18.Items.Clear();
            dg45.Items.Clear();
            DateTime dtCurrent = DateTime.Now;
            string text = string.Empty;

            _18ind_1.Fill = new SolidColorBrush(Colors.Red);
            _45ind_1.Fill = new SolidColorBrush(Colors.Red);
            _18ind_2.Fill = new SolidColorBrush(Colors.Red);
            _45ind_2.Fill = new SolidColorBrush(Colors.Red);
            _armyind_1.Fill = new SolidColorBrush(Colors.Red);
            _armyind_2.Fill = new SolidColorBrush(Colors.Red);

            Dictionary<DateTime, string> centers = new Dictionary<DateTime, string>();
            List<Session> listSession18 = new List<Session>();
            List<Session> listSession45 = new List<Session>();
            List<Session> listSessionArmy = new List<Session>();
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
                    Logger.WriteLog(System.DateTime.Now.ToString() + ":" + session.date + ":" + session.name + ":" + session.pincode + ":" + session.vaccine + ":" + session.min_age_limit + ":" + session.available_capacity);

                    if (session.name.IndexOf("only", StringComparison.CurrentCultureIgnoreCase) >= 0 && session.available_capacity > 0)
                    {
                        listSessionArmy.Add(session);
                    }
                    else if (session.available_capacity > 0)
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
            }

            if (listSession18.Count > 0)
            {
                var cols = GetIndicatorColors(listSession18);
                dg18.ItemsSource = listSession18;
                _18ind_1.Fill = cols[0];
                _18ind_2.Fill = cols[1];
                Alarm(5000, 500, 3, 50);
            }
            if (listSessionArmy.Count > 0)
            {
                dgArmy.ItemsSource = listSessionArmy;
                var cols = GetIndicatorColors(listSessionArmy);
                _armyind_1.Fill = cols[0];
                _armyind_2.Fill = cols[1];
            }

            if (listSession45.Count > 0)
            {
                dg45.ItemsSource = listSession45;
                var cols = GetIndicatorColors(listSession45);
                _45ind_1.Fill = cols[0];
                _45ind_2.Fill = cols[1];

                Alarm(8000, 1000, 2, 50);             
            }
            if (text.StartsWith("<!DOCTYPE"))
            {
                Alarm(2500, 3000, 1, 0);
                //Error too many requests sent.               
            }

            Alarm(1000, 500, 1, 0);
            
            pg1.Value = 0;
            pg1.IsIndeterminate = false;
            pg1.Orientation = Orientation.Horizontal;
           
            Duration duration = new Duration(TimeSpan.FromSeconds(10));
            int dur = 0;
            if (pg1.Value == 0)
            {
                dur = 10;
            }            

            DoubleAnimation doubleanimation = new DoubleAnimation(dur, duration);
            pg1.BeginAnimation(ProgressBar.ValueProperty, doubleanimation);

        }

        private static void Alarm(uint freq, uint duration, uint repeat,int gap)
        {
            for (int i = 0; i < repeat; i++)
            {
                System.Threading.Thread thread = new System.Threading.Thread(
        new System.Threading.ThreadStart(
            delegate ()
            {
                Beep(freq, duration);
                Thread.Sleep(gap);
            }
        ));

                thread.Start();
            }
        }

            private static List<SolidColorBrush> GetIndicatorColors( List<Session> sessions)
        {
            List<SolidColorBrush> colors = new List<SolidColorBrush>();
           var x = from session in sessions
                    where session.available_capacity_dose1 > 0
                    select session;

            if (x.Any())
            {
                colors.Add(new SolidColorBrush(Colors.ForestGreen));
            }
            else
            {
                colors.Add(new SolidColorBrush(Colors.Red));
            }

            var y = from session in sessions
                    where session.available_capacity_dose2 > 0
                    select session;

            if (y.Any())
            {
                colors.Add(new SolidColorBrush(Colors.ForestGreen));
            }
            else
            {
                colors.Add(new SolidColorBrush(Colors.Red));
            }
            return colors;
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            dispatcherTimer.Start();
            disCombo.IsEnabled = false;
            stCombo.IsEnabled = false;

            pg1.IsIndeterminate = false;
            pg1.Orientation = Orientation.Horizontal;
           
            Duration duration = new Duration(TimeSpan.FromSeconds(10));
            DoubleAnimation doubleanimation = new DoubleAnimation(10, duration);
            pg1.BeginAnimation(ProgressBar.ValueProperty, doubleanimation);
            

        }

        private void MonitorChkBox_Unchecked(object sender, RoutedEventArgs e)
        {
            dispatcherTimer.Stop();
            disCombo.IsEnabled = true;
            stCombo.IsEnabled = true;
            pg1.Value = 0;
        }
    }
}
