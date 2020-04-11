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
using System.Windows.Navigation;
using System.Windows.Shapes;
using TelloDroneController.src;

namespace TelloDroneController
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private TelloClient client;
        private const string DRONE_SIMULATOR_IP = "127.0.0.1";
        private const string DRONE_IP = "192.168.10.1";

        private void Init()
        {
            try
            {
                string ip = txt_drone_ip.Text;
                client = new TelloClient(ip);
                client.AddReceiver(ProcessDroneMessage);
                client.AddStatusReceiver(DroneStatusHandler);
            }
            catch (Exception e)
            {
                txt_response.Text = e.Message;
            }
        }

        private void window_drone_controller_KeyDown(object sender, KeyEventArgs e)
        {
            SwitchKeyEvent(true, e);
        }


        private void window_drone_controller_KeyUp(object sender, KeyEventArgs e)
        {
            SwitchKeyEvent(false, e);
        }

        private void SwitchKeyEvent(bool KeyDown, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.W: HandleKeyEvent(KeyDown, img_forward_gray, Command.FORWARD, "20"); break;
                case Key.S: HandleKeyEvent(KeyDown, img_backward_gray, Command.BACK, "20"); break;
                case Key.A: HandleKeyEvent(KeyDown, img_left_gray, Command.LEFT, "20"); break;
                case Key.D: HandleKeyEvent(KeyDown, img_right_gray, Command.RIGHT, "20"); break;
                case Key.Up: HandleKeyEvent(KeyDown, img_up_gray, Command.UP, "20"); break;
                case Key.Down: HandleKeyEvent(KeyDown, img_down_gray, Command.DOWN, "20"); break;
                case Key.Left: HandleKeyEvent(KeyDown, img_ccw_gray, Command.ROTATE_COUNTER_CLOCKWISE, "100"); break;
                case Key.Right: HandleKeyEvent(KeyDown, img_cw_gray, Command.ROTATE_CLOCKWISE, "100"); break;
                case Key.Space: HandleKeyEvent(KeyDown, img_land_gray, Command.LAND); break;
                case Key.Escape: HandleKeyEvent(KeyDown, img_emergency_gray, Command.EMERGENCY); break;
                case Key.Enter: HandleKeyEvent(KeyDown, img_takeoff_gray, Command.TAKEOFF); break;
            }
        }

        private void HandleKeyEvent(bool KeyDown, Image ActionImage, Command DroneCommand, string Parameter = "")
        {
            if (KeyDown) ActionImage.Visibility = Visibility.Hidden;
            else ActionImage.Visibility = Visibility.Visible;
            if (client != null && KeyDown) client.SendCommand(DroneCommand, Parameter);
        }

        private void ProcessDroneMessage(string SenderHostAddress, int SenderPort, string LastCommand, string Response)
        {
            this.Dispatcher.Invoke(new Action(() =>
            {
                if (Response == "ok") txt_response.Background = new SolidColorBrush((Color)App.Current.TryFindResource("color_green"));
                else txt_response.Background = new SolidColorBrush((Color)App.Current.TryFindResource("color_red"));
                txt_response.Text = String.Format("Processing response of [{1}] command: {0}", Response, LastCommand);
            }));
            
        }

        private void DroneStatusHandler(Dictionary<string, string> StatusValues)
        {
            this.Dispatcher.Invoke(new Action(() =>
            {
                SetBattery(int.Parse(StatusValues["bat"]));
                status_height.Content = Math.Round(int.Parse(StatusValues["h"]) / 100.0, 2);
                status_speed.Content = StatusValues["vgx"];
                status_htemp.Content = StatusValues["temph"];
            }));
        }

        private void SetBattery(int Value)
        {
            txt_battery.Text = Value.ToString();
            if (Value > 80) txt_battery.Background = new SolidColorBrush(Colors.Green);
            else if (Value > 50) txt_battery.Background = new SolidColorBrush(Colors.GreenYellow);
            else if (Value > 20) txt_battery.Background = new SolidColorBrush(Colors.Orange);
            else txt_battery.Background = new SolidColorBrush(Colors.Red);
        }

        private void window_drone_controller_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (client != null)
            {
                client.RemoveReceiver(ProcessDroneMessage);
                client.RemoveStatusReceiver(DroneStatusHandler);
            }
        }

        private void btn_start_Click(object sender, RoutedEventArgs e)
        {
            Init();
            client.SendCommand(Command.COMMAND);
            btn_start.IsEnabled = false;
        }
    }
}