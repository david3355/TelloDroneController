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
                txt_current_speed.Content = client.CurrentSpeed;
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
            if (client == null) return;
            int defaultSpeed = client.CurrentSpeed;
            try
            {
                switch (e.Key)
                {
                    case Key.W: HandleKeyEvent(KeyDown, img_forward_gray, TelloCommand.Forward.GetCommand(defaultSpeed)); break;
                    case Key.S: HandleKeyEvent(KeyDown, img_backward_gray, TelloCommand.Back.GetCommand(defaultSpeed)); break;
                    case Key.A: HandleKeyEvent(KeyDown, img_left_gray, TelloCommand.Left.GetCommand(defaultSpeed)); break;
                    case Key.D: HandleKeyEvent(KeyDown, img_right_gray, TelloCommand.Right.GetCommand(defaultSpeed)); break;
                    case Key.Up: HandleKeyEvent(KeyDown, img_up_gray, TelloCommand.Up.GetCommand(defaultSpeed)); break;
                    case Key.Down: HandleKeyEvent(KeyDown, img_down_gray, TelloCommand.Down.GetCommand(defaultSpeed)); break;
                    case Key.Left: HandleKeyEvent(KeyDown, img_ccw_gray, TelloCommand.RotateCounterClockwise.GetCommand(100)); break;
                    case Key.Right: HandleKeyEvent(KeyDown, img_cw_gray, TelloCommand.RotateClockwise.GetCommand(100)); break;
                    case Key.Space: HandleKeyEvent(KeyDown, img_land_gray, TelloCommand.Land.GetCommand()); break;
                    case Key.Escape: HandleKeyEvent(KeyDown, img_emergency_gray, TelloCommand.Emergency.GetCommand()); break;
                    case Key.Enter: HandleKeyEvent(KeyDown, img_takeoff_gray, TelloCommand.TakeOff.GetCommand()); break;

                    case Key.Q: HandleKeyEvent(KeyDown, img_dec_speed_gray); if (KeyDown) client.DecreaseSpeed(); break;
                    case Key.E: HandleKeyEvent(KeyDown, img_inc_speed_gray); if (KeyDown) client.IncreaseSpeed(); break;
                }
            }
            catch (CommandIntegerParamException ie)
            {
                txt_response.Text = ie.Message;
                txt_response.Background = new SolidColorBrush((Color)App.Current.TryFindResource("color_red"));
            }
            catch (CommandStringParamException se)
            {
                txt_response.Text = se.Message;
                txt_response.Background = new SolidColorBrush((Color)App.Current.TryFindResource("color_red"));
            }
            catch (DroneCurveException ce)
            {
                txt_response.Text = ce.Message;
                txt_response.Background = new SolidColorBrush((Color)App.Current.TryFindResource("color_red"));
            }
            txt_current_speed.Content = client.CurrentSpeed;
        }

        private void HandleKeyEvent(bool KeyDown, Image ActionImage, string DroneCommand=null)
        {
            if (KeyDown) ActionImage.Visibility = Visibility.Hidden;
            else ActionImage.Visibility = Visibility.Visible;
            if (client != null && KeyDown && DroneCommand != null) client.SendCommand(DroneCommand);
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
            client.SendCommand(TelloCommand.Command.GetCommand());
            btn_start.IsEnabled = false;
            menu_curve_editor.IsEnabled = true;
        }

        private void menu_curve_editor_Click(object sender, RoutedEventArgs e)
        {
            CurveEditor ceditor = new CurveEditor(client);
            ceditor.Show();
        }
    }
}