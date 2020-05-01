using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using TelloDroneController.src;

namespace TelloDroneController
{
    /// <summary>
    /// Interaction logic for Queries.xaml
    /// </summary>
    public partial class Queries : Window
    {
        public Queries(TelloClient Client)
        {
            InitializeComponent();
            this.client = Client;
            list_queries.ItemsSource = queries;
            list_queries.SelectedIndex = 0;
        }

        private TelloClient client;

        private string[] queries = { "Speed", "Battery", "Time", "Wifi signal strength", "SDK Version", "Serial number" };

        private object SwitchQuery(int QueryIndex)
        {
            switch (QueryIndex)
            {
                case 0: return client.GetSpeed(); 
                case 1: return client.GetBattery();
                case 2: return client.GetTime();
                case 3: return client.GetWifiSignalStrength();
                case 4: return client.GetSDK();
                case 5: return client.GetSerialNumber();
                default: return String.Empty;
            }
        }

        private void btn_query_Click(object sender, RoutedEventArgs e)
        {
            int selectedIndex = list_queries.SelectedIndex;
            if(selectedIndex != -1) txt_query_result.Text = SwitchQuery(selectedIndex).ToString();
        }
    }
}
