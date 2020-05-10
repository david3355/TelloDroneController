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
using System.Threading;
using System.Windows.Threading;
using System.IO;

namespace TelloDroneController
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, ITelloEvents
    {
        public MainWindow()
        {
            InitializeComponent();
            leftJoystick = new Joystick(img_left_joystick, left_joystick_panel);
            rightJoystick = new Joystick(img_right_joystick, right_joystick_panel);
            joystickAdjuster = new DispatcherTimer();
            joystickAdjuster.Interval = new TimeSpan(15000);
            joystickAdjuster.Tick += joystickAdjuster_Tick;
            joystickAdjuster.Start();
            red = new SolidColorBrush((Color)App.Current.TryFindResource("color_red"));
            green = new SolidColorBrush((Color)App.Current.TryFindResource("color_green"));
            defaultIps = new List<string>() { "127.0.0.1", "192.168.1.166", "192.168.10.1" };
            list_ips.ItemsSource = defaultIps;
            txt_drone_ip.Text = defaultIps[0];
            status = new DroneStatus();
            videoReceiverStarted = false;

            joystickDataSender = new DispatcherTimer();
            joystickDataSender.Tick += JoystickControl;
            joystickDataSender.Interval = new TimeSpan(0, 0, 0, 0, 500);
            SetJoystickMode();
        }

        private TelloClient client;
        private SolidColorBrush red, green;
        private List<string> defaultIps;
        private DispatcherTimer joystickAdjuster;
        private Joystick leftJoystick, rightJoystick;
        private bool joystickMode;
        private DispatcherTimer joystickDataSender;
        private int escapeTimes = 0;
        private DroneStatus status;
        private bool videoReceiverStarted;

        private bool Init()
        {
            string ip = txt_drone_ip.Text;
            if (AvailabilityChecker.IsAddressAvailable(ip))
            {
                if (client == null) client = new TelloClient(ip, this);
                txt_current_speed.Content = client.CurrentSpeed;
                return true;
            }
            MessageBox.Show(String.Format("Drone host is not available: {0}", ip));
            return false;
        }

        void joystickAdjuster_Tick(object sender, EventArgs e)
        {
            var controllerKeysDown = ControllerKeysDown();
            leftJoystick.AutoAdjust(controllerKeysDown.Contains(Key.A) || controllerKeysDown.Contains(Key.D), controllerKeysDown.Contains(Key.W) || controllerKeysDown.Contains(Key.S));
            rightJoystick.AutoAdjust(controllerKeysDown.Contains(Key.Left) || controllerKeysDown.Contains(Key.Right), controllerKeysDown.Contains(Key.Up) || controllerKeysDown.Contains(Key.Down));
            AdjustJoystick();
        }

        public static IEnumerable<Key> ControllerKeysDown()
        {
            Key[] controllerKeys = {Key.W, Key.A, Key.S, Key.D, Key.Up, Key.Left, Key.Down, Key.Right };
            foreach (Key key in controllerKeys)
            {
                if (Keyboard.IsKeyDown(key))
                    yield return key;
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

        private void JoystickControl(object sender, EventArgs e)
        {
            client.RemoteControl(leftJoystick.XPosition, leftJoystick.YPosition, rightJoystick.YPosition, rightJoystick.XPosition);
        }

        private void AdjustJoystick()
        {
            if (!joystickMode) return;
            bool shiftDown = Keyboard.IsKeyDown(Key.LeftShift);

            if (Keyboard.IsKeyDown(Key.W)) leftJoystick.PullUp();
            if (Keyboard.IsKeyDown(Key.S)) leftJoystick.PullDown();
            if (Keyboard.IsKeyDown(Key.A)) leftJoystick.PullLeft();
            if (Keyboard.IsKeyDown(Key.D)) leftJoystick.PullRight();
            if (Keyboard.IsKeyDown(Key.Up) && !shiftDown) rightJoystick.PullUp();
            if (Keyboard.IsKeyDown(Key.Down) && !shiftDown) rightJoystick.PullDown();
            if (Keyboard.IsKeyDown(Key.Left) && !shiftDown) rightJoystick.PullLeft();
            if (Keyboard.IsKeyDown(Key.Right) && !shiftDown) rightJoystick.PullRight();
        }

        private void SwitchKeyEvent(bool KeyDown, KeyEventArgs e)
        {
            if (client == null) return;
            int defaultDistance = 35;
            int defaultTurnDegree = 20;

            bool shiftDown = Keyboard.IsKeyDown(Key.LeftShift);

            if (joystickMode && !joystickDataSender.IsEnabled && e.Key == Key.Up && !shiftDown) joystickDataSender.Start();

            FlipShift(shiftDown);

            if (KeyDown && e.Key != Key.Escape)
            {
                escapeTimes = 0;
                emergency_notification.Visibility = Visibility.Collapsed;
            }

            try
            {
                switch (e.Key)
                {
                    case Key.W: HandleKeyEvent(KeyDown, img_forward_gray); if (KeyDown && !joystickMode) client.Forward(defaultDistance); break;
                    case Key.S: HandleKeyEvent(KeyDown, img_backward_gray); if (KeyDown && !joystickMode) client.Backward(defaultDistance); break;
                    case Key.A: HandleKeyEvent(KeyDown, img_left_gray); if (KeyDown && !joystickMode) client.Left(defaultDistance); break;
                    case Key.D: HandleKeyEvent(KeyDown, img_right_gray); if (KeyDown && !joystickMode) client.Right(defaultDistance); break;
                    case Key.Up:
                        HandleKeyEvent(KeyDown, img_up_gray, shiftDown, img_flip_forward);
                        if (KeyDown && !joystickMode && !shiftDown) client.Up(defaultDistance);
                        if (KeyDown && shiftDown) client.Flip(FlipDirection.FORWARD);
                        break;
                    case Key.Down: 
                        HandleKeyEvent(KeyDown, img_down_gray, shiftDown, img_flip_backward); 
                        if (KeyDown && !joystickMode && !shiftDown) client.Down(defaultDistance);
                        if (KeyDown && shiftDown) client.Flip(FlipDirection.BACKWARD);
                        break;
                    case Key.Left: 
                        HandleKeyEvent(KeyDown, img_ccw_gray, shiftDown, img_flip_left); 
                        if (KeyDown && !joystickMode && !shiftDown) client.TurnLeft(defaultTurnDegree);
                        if (KeyDown && shiftDown) client.Flip(FlipDirection.LEFT);
                        break;
                    case Key.Right:
                        HandleKeyEvent(KeyDown, img_cw_gray, shiftDown, img_flip_right); 
                        if (KeyDown && !joystickMode && !shiftDown) client.TurnRight(defaultTurnDegree);
                        if (KeyDown && shiftDown) client.Flip(FlipDirection.RIGHT);
                        break;
                    case Key.Space: HandleKeyEvent(KeyDown, img_land_gray); if (KeyDown) client.Land(); joystickDataSender.Stop(); break;
                    case Key.Escape: HandleKeyEvent(KeyDown, img_emergency_gray); if (KeyDown) HandleEmergency(); break;
                    case Key.Enter: HandleKeyEvent(KeyDown, img_takeoff_gray); if (KeyDown) client.TakeOff(); break;
                    case Key.LeftCtrl: HandleKeyEvent(KeyDown, img_start_rotors_gray); if (KeyDown) client.StartRotors(); break;

                    case Key.Q: HandleKeyEvent(KeyDown, img_dec_speed_gray); if (KeyDown) client.DecreaseSpeed(); break;
                    case Key.E: HandleKeyEvent(KeyDown, img_inc_speed_gray); if (KeyDown) client.IncreaseSpeed(); break;
                    case Key.P: SwitchStream(KeyDown); break;
                }
            }
            catch (CommandIntegerParamException ie)
            {
                txt_response.Text = ie.Message;
                txt_response.Background = red;
            }
            catch (CommandStringParamException se)
            {
                txt_response.Text = se.Message;
                txt_response.Background = red;
            }
            catch (DroneCurveException ce)
            {
                txt_response.Text = ce.Message;
                txt_response.Background = red;
            }
            txt_current_speed.Content = client.CurrentSpeed;
        }

        private void FlipShift(bool ShiftKeyDown)
        {
            if (ShiftKeyDown)
            {
                img_flip_forward.Visibility = Visibility.Visible;
                img_flip_backward.Visibility = Visibility.Visible;
                img_flip_left.Visibility = Visibility.Visible;
                img_flip_right.Visibility = Visibility.Visible;

                img_up.Visibility = Visibility.Collapsed;
                img_down.Visibility = Visibility.Collapsed;
                img_cw.Visibility = Visibility.Collapsed;
                img_ccw.Visibility = Visibility.Collapsed;
                img_up_gray.Visibility = Visibility.Collapsed;
                img_down_gray.Visibility = Visibility.Collapsed;
                img_cw_gray.Visibility = Visibility.Collapsed;
                img_ccw_gray.Visibility = Visibility.Collapsed;
            }
            else
            {
                img_flip_forward.Visibility = Visibility.Collapsed;
                img_flip_backward.Visibility = Visibility.Collapsed;
                img_flip_left.Visibility = Visibility.Collapsed;
                img_flip_right.Visibility = Visibility.Collapsed;

                img_up.Visibility = Visibility.Visible;
                img_down.Visibility = Visibility.Visible;
                img_cw.Visibility = Visibility.Visible;
                img_ccw.Visibility = Visibility.Visible;
                img_up_gray.Visibility = Visibility.Visible;
                img_down_gray.Visibility = Visibility.Visible;
                img_cw_gray.Visibility = Visibility.Visible;
                img_ccw_gray.Visibility = Visibility.Visible;
            }
        }

        public static bool StartVideoReceiver(int Width, int Height)
        {
            string receiverScript = "video_receiver.py";
            try
            {
                if (File.Exists(receiverScript))
                {
                    System.Diagnostics.Process.Start("python", String.Format("{0} {1} {2}", receiverScript, Width, Height));
                    return true;
                }
                else MessageBox.Show(String.Format("Video receiver script not found: {0}", receiverScript));
            }
            catch (Exception e)
            {
                MessageBox.Show("Failed to start video receiver! Details: " + e.Message);
            }
            return false;
        }

        private void SwitchStream(bool KeyDown)
        {
            if (client != null && KeyDown)
            {
                if (client.IsStreamOn)
                {
                    if (client.StreamOff()) img_stream_gray.Visibility = Visibility.Visible;
                }
                else
                {
                    if (client.StreamOn())
                    {
                        if (!videoReceiverStarted) 
                        {
                            // Must start on new thread otherwise wrong keyevent is raised!
                            new Thread(() => 
                                {
                                    if (StartVideoReceiver(640, 480)) videoReceiverStarted = true;
                                }).Start();
                        } 
                        img_stream_gray.Visibility = Visibility.Collapsed;
                    }
                }
            }
        }

        private void HandleKeyEvent(bool KeyDown, Image ActionImage, bool ShiftDown = false, Image ShiftActionImage = null)
        {
            if (ShiftDown)
            {
                if (ShiftActionImage != null)
                {
                    if (KeyDown) ShiftActionImage.Margin = new Thickness(0);
                    else ShiftActionImage.Margin = new Thickness(5);
                }
            }
            else
            {
                if (KeyDown) ActionImage.Visibility = Visibility.Hidden;
                else ActionImage.Visibility = Visibility.Visible;
            }
        }

        private void HandleEmergency()
        {
            emergency_notification.Visibility = Visibility.Visible;
            if (escapeTimes == 0) txt_emergency_info.Text = "Press Escape again to stop all motors!";
            else if (escapeTimes == 1 && status.HeightCM > 100) txt_emergency_info.Text = "Drone is higher than 1 meter! Press Escape again to stop all motors!";
            else emergency_notification.Visibility = Visibility.Collapsed;

            if (status.HeightCM < 100 && escapeTimes > 0) Emergency();
            else if (escapeTimes > 1) Emergency();
            escapeTimes++;
        }

        private void LockJoystick()
        {
            if (joystickMode)
            {
                leftJoystick.LockStillPosition();
                rightJoystick.LockStillPosition();
            }
        }

        private void UnlockJoystick()
        {
            if (joystickMode)
            {
                leftJoystick.Unlock();
                rightJoystick.Unlock();
            }
        }

        private void Emergency()
        {
            LockJoystick();
            client.MakeDroneStill();
            client.Emergency();
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
            if (joystickDataSender != null) joystickDataSender.Stop();
            if (joystickAdjuster != null) joystickAdjuster.Stop();
        }

        private void btn_start_Click(object sender, RoutedEventArgs e)
        {
            if (Init())
            {
                client.Command();
                btn_start.IsEnabled = false;
                txt_drone_ip.IsEnabled = false;
                btn_ip_list.IsEnabled = false;
                check_joystick_mode.IsEnabled = false;
                menu_curve_editor.IsEnabled = true;
                menu_query.IsEnabled = true;
            }
        }

        private void menu_curve_editor_Click(object sender, RoutedEventArgs e)
        {
            CurveEditor ceditor = new CurveEditor(client);
            ceditor.Show();
        }

        public void ReceiveTelloResponse(string SenderHostAddress, int SenderPort, string LastCommand, string Response)
        {
            if (LastCommand == TelloCommand.Emergency.GetCommand() && Response == DroneResponseValue.OK)
            {
                joystickDataSender.Stop();
                UnlockJoystick();
            }

            this.Dispatcher.Invoke(new Action(() =>
            {
                if (Response == DroneResponseValue.OK) txt_response.Background = green;
                else txt_response.Background = red;
                txt_response.Text = String.Format("Processing response of [{1}] command: {0}", Response, LastCommand);
            }));
        }

        public void ReceiveDroneStatus(Dictionary<string, string> StatusValues)
        {
            this.Dispatcher.Invoke(new Action(() =>
            {
                status.Parse(StatusValues);
                SetBattery(status.Battery);
                status_height.Content = Math.Round(status.HeightCM / 100.0, 2);
                status_speed.Content = status.SpeedX;
                status_htemp.Content = status.HighestTemperature;
            }));
        }

        public void TelloConnectionError(string ErrorMessage)
        {
            this.Dispatcher.Invoke(new Action(() =>
            {
                txt_response.Background = red;
                txt_response.Text = ErrorMessage;
                btn_start.IsEnabled = true;
                menu_curve_editor.IsEnabled = false;
            }));
        }

        public void CommandSent(string DroneCommand, bool Async)
        {
            this.Dispatcher.Invoke(new Action(() =>
            {
                txt_command.Text = String.Format("({0}) {1}", Async ? "A" : "S", DroneCommand);
            }));
        }

        private void btn_ip_list_Click(object sender, RoutedEventArgs e)
        {
            list_ips.Visibility = Visibility.Visible;
            txt_drone_ip.Visibility = Visibility.Collapsed;
            btn_ip_list.IsEnabled = false;
            list_ips.IsDropDownOpen = true;
        }

        private void list_ips_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            txt_drone_ip.Text = defaultIps[list_ips.SelectedIndex];
            list_ips.Visibility = Visibility.Collapsed;
            txt_drone_ip.Visibility = Visibility.Visible;
            btn_ip_list.IsEnabled = true;
        }

        private void SetJoystickMode()
        {
            if (img_left_joystick == null || img_right_joystick == null || img_left_joystick_gray == null || img_right_joystick_gray == null) return;
            if (joystickMode)
            {
                img_left_joystick.Visibility = Visibility.Visible;
                img_right_joystick.Visibility = Visibility.Visible;
                img_left_joystick_gray.Visibility = Visibility.Collapsed;
                img_right_joystick_gray.Visibility = Visibility.Collapsed;
            }
            else
            {
                if (client != null) client.RemoteControl(0, 0, 0, 0); // TODO is it necessary?
                joystickDataSender.Stop();
                img_left_joystick.Visibility = Visibility.Collapsed;
                img_right_joystick.Visibility = Visibility.Collapsed;
                img_left_joystick_gray.Visibility = Visibility.Visible;
                img_right_joystick_gray.Visibility = Visibility.Visible;
            }
        }

        private void check_joystick_mode_Check_changed(object sender, RoutedEventArgs e)
        {
            joystickMode = check_joystick_mode.IsChecked.Value;
            SetJoystickMode();
        }

        private void menu_query_Click(object sender, RoutedEventArgs e)
        {
            Queries queries = new Queries(client);
            queries.Show();
        }

        private void menu_start_video_receiver_Click(object sender, RoutedEventArgs e)
        {
            VideoReceiverStarter vrs = new VideoReceiverStarter();
            vrs.Show();
        }
    }
}