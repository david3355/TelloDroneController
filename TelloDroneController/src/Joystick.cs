using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace TelloDroneController.src
{
    class Joystick
    {
        public Joystick(FrameworkElement JoystickImage, Canvas Background)
        {
            image = JoystickImage;
            xPos = 0;
            yPos = 0;
            locked = false;
            imgWidth = JoystickImage.Width;
            imgHeight = JoystickImage.Height;
            originalXPos = Background.Width / 2 - imgWidth / 2;
            originalYPos = Background.Height / 2 - imgHeight / 2;
            AdjustImage();
        }

        private FrameworkElement image;
        private double xPos, yPos;

        private const int DEFAULT_BOUNDARY = 100;

        private const int maxXPos = DEFAULT_BOUNDARY;
        private const int maxYPos = DEFAULT_BOUNDARY;
        private const int minXPos = -DEFAULT_BOUNDARY;
        private const int minYPos = -DEFAULT_BOUNDARY;

        private const int ADJUST_MOVE_UNIT_KEYDOWN = 0;
        private const int ADJUST_MOVE_UNIT_NO_KEYDOWN = 10;

        private double originalXPos, originalYPos;
        private double imgWidth, imgHeight;
        private bool locked;

        private enum Axis { X, Y }

        public int XPosition
        {
            get { return (int)xPos; }
        }

        public int YPosition
        {
            get { return (int)yPos; }
        }

        public void PullUp()
        {
            if (locked) return;
            yPos += GetMoveUnit(yPos, 1);
            AdjustImage();
        }

        public void PullDown()
        {
            if (locked) return;
            yPos += GetMoveUnit(yPos, -1);
            AdjustImage();
        }

        public void PullRight()
        {
            if (locked) return;
            xPos += GetMoveUnit(xPos, 1);
            AdjustImage();
        }

        public void PullLeft()
        {
            if (locked) return;
            xPos += GetMoveUnit(xPos, -1);
            AdjustImage();
        }

        public void LockStillPosition()
        {
            xPos = 0;
            yPos = 0;
            AdjustImage();
            locked = true;
        }

        public void Unlock()
        {
            locked = false;
        }

        private double JoystickForceFunction(double ActualPosition)
        {
            if (ActualPosition < 10) return 10;
            else if (ActualPosition < 20) return 9;
            else if (ActualPosition < 30) return 8;
            else if (ActualPosition < 40) return 5;
            else if (ActualPosition < 50) return 4;
            else if (ActualPosition < 60) return 2;
            else if (ActualPosition < 100) return 0.1;
            return 0;
        }

        private double GetMoveUnit(double ActualPos, int Direction)
        {
            double moveUnit = JoystickForceFunction(Math.Abs(ActualPos));
            if (Math.Abs(ActualPos) + moveUnit > DEFAULT_BOUNDARY) moveUnit = DEFAULT_BOUNDARY - Math.Abs((int)ActualPos);
            return Direction * moveUnit;
        }

        private int GetAutoAdjustUnit(bool IsXControllerKeyDown, bool IsYControllerKeyDown, double ActualPos, Axis Axis)
        {
            int adjustment = ADJUST_MOVE_UNIT_NO_KEYDOWN;
            if ((IsXControllerKeyDown && Axis == Axis.X) || (IsYControllerKeyDown && Axis == Axis.Y)) adjustment = ADJUST_MOVE_UNIT_KEYDOWN;
            if (Math.Abs(ActualPos) - adjustment < 0) adjustment = Math.Abs((int)ActualPos);
            return ActualPos > 0 ? -adjustment : adjustment;
        }

        public void AutoAdjust(bool IsXControllerKeyDown, bool IsYControllerKeyDown)
        {
            if (locked) return;
            xPos += GetAutoAdjustUnit(IsXControllerKeyDown, IsYControllerKeyDown, xPos, Axis.X);
            yPos += GetAutoAdjustUnit(IsXControllerKeyDown, IsYControllerKeyDown, yPos, Axis.Y);
            AdjustImage();
        }

        private void AdjustImage()
        {
            Canvas.SetTop(image, originalYPos - yPos / 2);
            Canvas.SetLeft(image, originalXPos + xPos / 2);
        }
    }
}
