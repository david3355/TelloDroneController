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
using TelloDroneController.src.curve;
using TelloDroneController.src;

namespace TelloDroneController
{
    /// <summary>
    /// Interaction logic for CurveEditor.xaml
    /// </summary>
    public partial class CurveEditor : Window
    {
        public CurveEditor(TelloClient Client)
        {
            InitializeComponent();
            client = Client;
            points = new List<Circle>();
            red = new SolidColorBrush(Colors.PaleVioletRed);
            green = new SolidColorBrush(Colors.LightGreen);
        }

        private TelloClient client;

        Curve circle;
        double oX, oY, R;

        Circle startPoint, P1, P2;
        Circle selectedPoint;
        List<Circle> points;

        private SolidColorBrush red, green;

        private void CalculateCircle(double x1, double y1, double x2, double y2, double x3, double y3)
        {
            double A = x1 * (y2 - y3) - y1 * (x2 - x3) + x2 * y3 - x3 * y2;
            double B = (Math.Pow(x1, 2) + Math.Pow(y1, 2)) * (y3 - y2) + (Math.Pow(x2, 2) + Math.Pow(y2, 2)) * (y1 - y3) + (Math.Pow(x3, 2) + Math.Pow(y3, 2)) * (y2 - y1);
            double C = (Math.Pow(x1, 2) + Math.Pow(y1, 2)) * (x2 - x3) + (Math.Pow(x2, 2) + Math.Pow(y2, 2)) * (x3 - x1) + (Math.Pow(x3, 2) + Math.Pow(y3, 2)) * (x1 - x2);
            double D = (Math.Pow(x1, 2) + Math.Pow(y1, 2)) * (x3 * y2 - x2 * y3) + (Math.Pow(x2, 2) + Math.Pow(y2, 2)) * (x1 * y3 - x3 * y1) + (Math.Pow(x3, 2) + Math.Pow(y3, 2)) * (x2 * y1 - x1 * y2);

            oX = -(B / (2 * A));
            oY = -(C / (2 * A));
            R = Math.Sqrt((Math.Pow(B, 2) + Math.Pow(C, 2) - 4 * A * D) / (4 * Math.Pow(A, 2)));
            btn_curve.IsEnabled = Validate();
        }

        private void SetCircle()
        {
            circle.Move(new Point(oX, oY), R);
        }

        private Circle ClickedTag(Point ClickPos)
        {
            foreach (Circle tag in points)
            {
                if (tag.Contains(ClickPos)) return tag;
            }
            return null;
        }

        private void Drawboard_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Point mousepos = e.GetPosition(drawboard);
            Circle previousSelecion = selectedPoint;
            selectedPoint = ClickedTag(mousepos);
            if (selectedPoint == null) selectedPoint = previousSelecion;
        }


        private void Drawboard_MouseMove(object sender, MouseEventArgs e)
        {
            Point mousepos = e.GetPosition(drawboard);
            if (e.LeftButton == MouseButtonState.Pressed && selectedPoint != null)
            {
                selectedPoint.MoveOnCanvas(mousepos);
                Calculate();
                SetCircle();
            }
        }

        private void Calculate()
        {
            double x1 = startPoint.Origo.X;
            double y1 = startPoint.Origo.Y;
            double x2 = P1.Origo.X; ;
            double y2 = P1.Origo.Y;
            double x3 = P2.Origo.X;
            double y3 = P2.Origo.Y;

            txt_x1.Content = Math.Round(x2);
            txt_y1.Content = Math.Round(y2);
            txt_x2.Content = Math.Round(x3);
            txt_y2.Content = Math.Round(y3);
            txt_r.Content = Math.Round(R);
            CalculateCircle(x1, y1, x2, y2, x3, y3);
        }

        private bool Validate()
        {
            bool valid = Valid(R, 50, 1000, txt_r);
            valid &= Valid(P1.Origo.X, 20, 500, txt_x1);
            valid &= Valid(P1.Origo.Y, 20, 500, txt_y1);
            valid &= Valid(P2.Origo.X, 20, 500, txt_x2);
            valid &= Valid(P2.Origo.Y, 20, 500, txt_y2);

            return valid;
        }

        private bool Valid(double Value, double LowerBound, double UpperBound, Label Display)
        {
            bool valid = LowerBound <= Value && Value <= UpperBound;
            Display.Background = valid ? green : red;
            return valid;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            const double margin = 10;
            double xmin = margin;
            double xmax = drawboard.Width - margin;
            double ymin = margin;
            double ymax = drawboard.Height - margin;
            const double step = 10;

            double canvasOrigoY = (ymin + ymax) / 2;
            double canvasOrigoX = (xmin + xmax) / 2;

            Circle.CanvasOffsetX = canvasOrigoX;
            Circle.CanvasOffsetY = canvasOrigoY;

            // Make the X axis.
            GeometryGroup xaxis_geom = new GeometryGroup();
            xaxis_geom.Children.Add(new LineGeometry(
                new Point(xmin, canvasOrigoY), new Point(drawboard.Width, canvasOrigoY)));
            for (double x = xmin + step;
                x <= drawboard.Width - step; x += step)
            {
                xaxis_geom.Children.Add(new LineGeometry(
                    new Point(x, canvasOrigoY - margin / 2),
                    new Point(x, canvasOrigoY + margin / 2)));
            }

            Path xaxis_path = new Path();
            xaxis_path.StrokeThickness = 1;
            xaxis_path.Stroke = Brushes.Black;
            xaxis_path.Data = xaxis_geom;

            drawboard.Children.Add(xaxis_path);

            // Make the Y ayis.
            GeometryGroup yaxis_geom = new GeometryGroup();
            yaxis_geom.Children.Add(new LineGeometry(
                new Point(canvasOrigoX, 0), new Point((xmin + xmax) / 2, drawboard.Height)));
            for (double y = step; y <= drawboard.Height - step; y += step)
            {
                yaxis_geom.Children.Add(new LineGeometry(
                    new Point((xmin + xmax) / 2 - margin / 2, y),
                    new Point((xmin + xmax) / 2 + margin / 2, y)));
            }

            Path yaxis_path = new Path();
            yaxis_path.StrokeThickness = 1;
            yaxis_path.Stroke = Brushes.Black;
            yaxis_path.Data = yaxis_geom;

            drawboard.Children.Add(yaxis_path);

            // Make some data sets.
            startPoint = new Circle(new Point(0, 0), drawboard, "S");
            startPoint.Init();
            startPoint.Fill(Colors.Green);
            P1 = new Circle(new Point(50, 150), drawboard, "P1");
            P1.Init();
            P2 = new Circle(new Point(175, 130), drawboard, "P2");
            P2.Init();
            points.Add(P1);
            points.Add(P2);

            Calculate();

            circle = new Curve(new Point(oX, oY), drawboard, R, "O");
            circle.Init();
        }

        private void btn_curve_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int speed = int.Parse(txt_speed.Text);
                client.SendCommand(TelloCommand.Curve.GetCommand((int)P1.Origo.X, (int)P1.Origo.Y, 0, (int)P2.Origo.X, (int)P2.Origo.Y, 0, speed));
            }
            catch (CommandIntegerParamException ie)
            {
                MessageBox.Show(ie.Message);
            }
            catch (DroneCurveException ce)
            {
                MessageBox.Show(ce.Message);
            }
        }
    }
}
