using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Shapes;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace TelloDroneController.src.curve
{
    public class Circle
    {
        public Circle(Point Origo, Canvas Background, string Name)
        {
            this.name = Name;
            this.origo = Origo;
            this.background = Background;
            red = new SolidColorBrush(Colors.Red);
            green = new SolidColorBrush(Colors.Green);
            purple = new SolidColorBrush(Colors.Purple);
            display = new Ellipse();
            distanceLabel = new Label();
            diameter = CIRCLE_DIAMETER;
        }

        protected string name;
        protected Point origo;
        protected Ellipse display;
        protected Canvas background;
        protected Label distanceLabel;

        protected static SolidColorBrush red, green, purple;
        protected double diameter;
        protected const double CIRCLE_DIAMETER = 15;
        protected const double LABEL_OFFSET = 5;

        public static double CanvasOffsetX = 0;
        public static double CanvasOffsetY = 0;

        public Point Origo
        {
            get { return origo; }
        }

        public Point CanvasOrigo
        {
            get { return new Point(origo.X + CanvasOffsetX, -origo.Y + CanvasOffsetY); }
        }

        public double Radius
        {
            get { return diameter / 2; }
        }


        public bool Contains(Point P)
        {
            return CanvasOrigo.X - diameter / 2 < P.X && P.X < CanvasOrigo.X + diameter / 2 && CanvasOrigo.Y - diameter / 2 < P.Y && P.Y < CanvasOrigo.Y + diameter / 2;
        }

        public void Fill(Color Color)
        {
            display.Fill = new SolidColorBrush(Color);
        }

        public virtual void Init()
        {
            display.Width = diameter;
            display.Height = diameter;
            display.Fill = red;

            distanceLabel.FontSize = 12;
            distanceLabel.Foreground = red;
            distanceLabel.Content = String.Format("{2} [{0};{1}]", Math.Round(Origo.X, 2), Math.Round(Origo.Y, 2), name);
            background.Children.Add(distanceLabel);
            Canvas.SetLeft(distanceLabel, CanvasOrigo.X + LABEL_OFFSET);
            Canvas.SetTop(distanceLabel, CanvasOrigo.Y + LABEL_OFFSET);
            
            background.Children.Add(display);
            Canvas.SetLeft(display, CanvasOrigo.X - diameter / 2);
            Canvas.SetTop(display, CanvasOrigo.Y - diameter / 2);
        }

        public void MoveOnCanvas(Point NewCanvasOrigo, double NewRadius)
        {
            Move(new Point(NewCanvasOrigo.X - CanvasOffsetX, -(NewCanvasOrigo.Y - CanvasOffsetY)), NewRadius);
        }

        public virtual void Move(Point NewOrigo, double NewRadius)
        {
            origo = new Point(NewOrigo.X, NewOrigo.Y);
            diameter = NewRadius * 2;
            distanceLabel.Content = String.Format("{2} [{0};{1}]", Math.Round(Origo.X, 2), Math.Round(Origo.Y, 2), name);
            Canvas.SetLeft(distanceLabel, CanvasOrigo.X + LABEL_OFFSET);
            Canvas.SetTop(distanceLabel, CanvasOrigo.Y + LABEL_OFFSET);
            Canvas.SetLeft(display, CanvasOrigo.X - diameter / 2);
            Canvas.SetTop(display, CanvasOrigo.Y - diameter / 2);
        }

        public void Move(Point NewOrigo)
        {
            Move(NewOrigo, Radius);
        }

        public void MoveOnCanvas(Point NewCanvasOrigo)
        {
            MoveOnCanvas(NewCanvasOrigo, Radius);
        }

    }

    public class Curve : Circle
    {
        public Curve(Point Origo, Canvas Background, double Radius, string Name): base(Origo, Background, Name)
        {
            diameter = Radius * 2;
            display.Fill = null;
            display.Stroke = green;
            origoDisplay = new Ellipse();
        }

        private Ellipse origoDisplay;
        private const double ORIGO_RADIUS = 3;

        public override void Init()
        {
            display.Width = diameter;
            display.Height = diameter;

            distanceLabel.FontSize = 12;
            distanceLabel.Foreground = purple;
            distanceLabel.Content = String.Format("{2} [{0};{1}] R: {3}", Math.Round(Origo.X, 1), Math.Round(Origo.Y, 1), name, Math.Round(Radius, 1));
            background.Children.Add(distanceLabel);

            Canvas.SetLeft(distanceLabel, CanvasOrigo.X + LABEL_OFFSET);
            Canvas.SetTop(distanceLabel, CanvasOrigo.Y + LABEL_OFFSET);

            background.Children.Add(display);
            Canvas.SetLeft(display, CanvasOrigo.X - diameter / 2);
            Canvas.SetTop(display, CanvasOrigo.Y - diameter / 2);
            Canvas.SetZIndex(display, -1);

            background.Children.Add(origoDisplay);
            origoDisplay.Width = ORIGO_RADIUS * 2;
            origoDisplay.Height = ORIGO_RADIUS * 2;
            origoDisplay.Fill = purple;
            Canvas.SetLeft(origoDisplay, CanvasOrigo.X - ORIGO_RADIUS);
            Canvas.SetTop(origoDisplay, CanvasOrigo.Y - ORIGO_RADIUS);
            Canvas.SetZIndex(origoDisplay, -1);
        }

        public override void Move(Point NewOrigo, double NewRadius)
        {
            if( Double.IsInfinity(NewOrigo.X) || Double.IsInfinity(NewOrigo.Y)) return;
            origo = new Point(NewOrigo.X, NewOrigo.Y);
            diameter = NewRadius * 2;
            distanceLabel.Content = String.Format("{2} [{0};{1}] R: {3}", Math.Round(Origo.X, 1), Math.Round(Origo.Y, 1), name, Math.Round(Radius, 1));
            Canvas.SetLeft(distanceLabel, CanvasOrigo.X + LABEL_OFFSET);
            Canvas.SetTop(distanceLabel, CanvasOrigo.Y + LABEL_OFFSET);
            Canvas.SetLeft(display, CanvasOrigo.X - diameter / 2);
            Canvas.SetTop(display, CanvasOrigo.Y - diameter / 2);
            display.Width = diameter;
            display.Height = diameter;

            Canvas.SetLeft(origoDisplay, CanvasOrigo.X - ORIGO_RADIUS);
            Canvas.SetTop(origoDisplay, CanvasOrigo.Y - ORIGO_RADIUS);
            Canvas.SetZIndex(origoDisplay, -1);
        }
    }
}
