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

namespace TelloDroneController
{
    /// <summary>
    /// Interaction logic for VideoReceiverStarter.xaml
    /// </summary>
    public partial class VideoReceiverStarter : Window
    {
        public VideoReceiverStarter()
        {
            InitializeComponent();
            txt_video_width.Text = "640";
            txt_video_height.Text = "480";
        }

        private void btn_start_video_receiver_Click(object sender, RoutedEventArgs e)
        {
            int width = int.Parse(txt_video_width.Text);
            int height = int.Parse(txt_video_height.Text);
            if (MainWindow.StartVideoReceiver(width, height)) this.Close();
        }
    }
}
