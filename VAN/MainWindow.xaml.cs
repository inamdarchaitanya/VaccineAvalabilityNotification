using Newtonsoft.Json;
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

        List<Session> listSession18d1;
        List<Session> listSession45d1;
        List<Session> listSessionArmyd1;

        List<Session> listSession18d2;
        List<Session> listSession45d2;
        List<Session> listSessionArmyd2;

        public MainWindow()
        {
            InitializeComponent();
            //  DispatcherTimer setup
            dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 10);
            DateTime dtCurrent = System.DateTime.Now;
            if (dtCurrent.TimeOfDay.Hours > 13)
            {
                dtCurrent = dtCurrent.AddDays(1);
            }
            dtpckr.SelectedDate = dtCurrent;
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
            dg18d1.ItemsSource = null;
            dg45d1.ItemsSource = null;
            dgResd1.ItemsSource = null;
            dgResd1.Items.Clear();
            dg18d1.Items.Clear();
            dg45d1.Items.Clear();

            dg18d2.ItemsSource = null;
            dg45d2.ItemsSource = null;
            dgResd2.ItemsSource = null;
            dgResd2.Items.Clear();
            dg18d2.Items.Clear();
            dg45d2.Items.Clear();

            listSession18d1 = new List<Session>();
            listSession45d1 = new List<Session>();
            listSessionArmyd1 = new List<Session>();

            listSession18d2 = new List<Session>();
            listSession45d2 = new List<Session>();
            listSessionArmyd2 = new List<Session>();


            DateTime dtCurrent = (DateTime)dtpckr.SelectedDate;
           
            string text = string.Empty;

            _18ind_1.Fill = new SolidColorBrush(Colors.Red);
            _45ind_1.Fill = new SolidColorBrush(Colors.Red);
            _18ind_2.Fill = new SolidColorBrush(Colors.Red);
            _45ind_2.Fill = new SolidColorBrush(Colors.Red);
            _resind_1.Fill = new SolidColorBrush(Colors.Red);
            _resind_2.Fill = new SolidColorBrush(Colors.Red);

            Dictionary<DateTime, string> centers = new Dictionary<DateTime, string>();


            for (int i = 0; i < int.Parse(((System.Windows.Controls.ContentControl)cmbdays.SelectedValue).Content.ToString()); i++)
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
                    if ((bool)Log_chkBox.IsChecked)
                    {
                        Logger.WriteLog(System.DateTime.Now.ToString() + ":" + session.date + ":" + session.name + ":" + session.pincode + ":" + session.vaccine + ":" + session.min_age_limit + ":" + session.available_capacity);
                    }
                    if (session.name.IndexOf("only", StringComparison.CurrentCultureIgnoreCase) >= 0 && session.available_capacity > 0)
                    {
                        if (session.available_capacity_dose1 > 0)
                        {
                            listSessionArmyd1.Add(session);
                        }
                        else
                        {
                            listSessionArmyd2.Add(session);
                        }
                    }
                    else if (session.available_capacity > 0)
                    {
                        if (session.min_age_limit == 18)
                        {
                            if (session.available_capacity_dose1 > 0)
                            {
                                listSession18d1.Add(session);
                            }
                            else
                            {
                                listSession18d2.Add(session);
                            }
                        }
                        else
                        {
                            if (session.available_capacity_dose1 > 0)
                            {
                                listSession45d1.Add(session);
                            }
                            else
                            {
                                listSession45d2.Add(session);
                            }
                        }
                    }
                }
            }

            if (listSession18d1.Count > 0)
            {              
                dg18d1.ItemsSource = listSession18d1;
                _18ind_1.Fill = new SolidColorBrush(Colors.ForestGreen);   
                BuzzAlarm(listSession18d1, rb18d1);
            }

            if (listSession18d2.Count > 0)
            {               
                dg18d2.ItemsSource = listSession18d2;
                _18ind_2.Fill = new SolidColorBrush(Colors.ForestGreen);
                BuzzAlarm(listSession18d2, rb18d2);
            }

            if (listSessionArmyd1.Count > 0)
            {
                dgResd1.ItemsSource = listSessionArmyd1;               
                _resind_1.Fill = new SolidColorBrush(Colors.ForestGreen);   
                BuzzAlarm(listSessionArmyd1, rbresd1);
            }

            if (listSessionArmyd2.Count > 0)
            {
                dgResd2.ItemsSource = listSessionArmyd2;
                _resind_2.Fill = new SolidColorBrush(Colors.ForestGreen);
                BuzzAlarm(listSessionArmyd2, rbresd2);
            }


            if (listSession45d1.Count > 0)
            {
                dg45d1.ItemsSource = listSession45d1;
                _45ind_1.Fill = new SolidColorBrush(Colors.ForestGreen);
                BuzzAlarm(listSession45d1, rb45d1);
            }

            if (listSession45d2.Count > 0)
            {
                dg45d2.ItemsSource = listSession45d2;
                _45ind_2.Fill = new SolidColorBrush(Colors.ForestGreen);
                BuzzAlarm(listSession45d2, rb45d2);
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

        private void BuzzAlarm(List<Session> sessions, RadioButton rb)
        {        
            if ((bool)rb.IsChecked)
                Alarm(5000, 500, 3, 50);           
        }

        private static void Alarm(uint freq, uint duration, uint repeat, int gap)
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

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (stCombo.SelectedItem != null && disCombo.SelectedItem != null)
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

        }

        private void MonitorChkBox_Unchecked(object sender, RoutedEventArgs e)
        {
            dispatcherTimer.Stop();
            disCombo.IsEnabled = true;
            stCombo.IsEnabled = true;
            pg1.Value = 0;
        }

        private void Cb18d1_Click(object sender, RoutedEventArgs e)
        {
            var maxObject = listSession18d1.OrderByDescending(item => item.available_capacity_dose1).First();
            SetPINInClipboard(maxObject);
        }

       

        private void Cb18d2_Click(object sender, RoutedEventArgs e)
        {
            var maxObject = listSession18d2.OrderByDescending(item => item.available_capacity_dose2).First();
            SetPINInClipboard(maxObject);
        }

        private void Cb45d1_Click(object sender, RoutedEventArgs e)
        {
            var maxObject = listSession45d1.OrderByDescending(item => item.available_capacity_dose1).First();
            SetPINInClipboard(maxObject);
        }

        private void Cb45d2_Click(object sender, RoutedEventArgs e)
        {
            var maxObject = listSession45d2.OrderByDescending(item => item.available_capacity_dose2).First();
            SetPINInClipboard(maxObject);
        }

        private void Cbresd1_Click(object sender, RoutedEventArgs e)
        {
            var maxObject = listSessionArmyd1.OrderByDescending(item => item.available_capacity_dose1).First();
            SetPINInClipboard(maxObject);
        }

        private void Cbresd2_Click(object sender, RoutedEventArgs e)
        {
            var maxObject = listSessionArmyd2.OrderByDescending(item => item.available_capacity_dose2).First();
            SetPINInClipboard(maxObject);
        }

        private void SetPINInClipboard(Session maxObject)
        {
            Clipboard.SetText(maxObject.pincode.ToString());
        }
    }
}
