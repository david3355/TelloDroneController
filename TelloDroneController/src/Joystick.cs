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
            imgWidth = JoystickImage.Width;
            imgHeight = JoystickImage.Height;
            originalXPos = Background.Width / 2 - imgWidth / 2;
            originalYPos = Background.Height / 2 - imgHeight / 2;
            AdjustImage();
        }

        private FrameworkElement image;
        private int xPos, yPos;

        private const int DEFAULT_BOUNDARY = 100;

        private const int maxXPos = DEFAULT_BOUNDARY;
        private const int maxYPos = DEFAULT_BOUNDARY;
        private const int minXPos = -DEFAULT_BOUNDARY;
        private const int minYPos = -DEFAULT_BOUNDARY;

        private const int ADJUST_MOVE_UNIT_KEYDOWN = 0;
        private const int ADJUST_MOVE_UNIT_NO_KEYDOWN = 10;

        private double originalXPos, originalYPos;
        private double imgWidth, imgHeight;

        public int XPosition
        {
            get { return xPos; }
        }

        public int YPosition
        {
            get { return yPos; }
        }

        public void PullUp()
        {
            yPos += GetMoveUnit(yPos, 1);
            AdjustImage();
        }

        public void PullDown()
        {
            yPos += GetMoveUnit(yPos, -1);
            AdjustImage();
        }

        public void PullRight()
        {
            xPos += GetMoveUnit(xPos, 1);
            AdjustImage();
        }

        public void PullLeft()
        {
            xPos += GetMoveUnit(xPos, -1);
            AdjustImage();
        }

        private int GetMoveUnit(int ActualPos, int Direction)
        {
            int max = (int)Math.Round(Math.Sqrt(DEFAULT_BOUNDARY), 0);
            int square = (int)Math.Sqrt(Math.Abs(ActualPos));
            int moveUnit = max - square;
            if (Math.Abs(ActualPos) + moveUnit > DEFAULT_BOUNDARY) moveUnit = DEFAULT_BOUNDARY - Math.Abs(ActualPos);
            return Direction * moveUnit;
        }

        private int GetAutoAdjustUnit(bool IsAnyControllerKeyDown, int ActualPos)
        {
            int adjustment = ADJUST_MOVE_UNIT_NO_KEYDOWN;
            if (IsAnyControllerKeyDown) adjustment = ADJUST_MOVE_UNIT_KEYDOWN;
            if (Math.Abs(ActualPos) - adjustment < 0) adjustment = Math.Abs(ActualPos);
            return ActualPos > 0 ? -adjustment : adjustment;
        }

        public void AutoAdjust(bool IsAnyControllerKeyDown)
        {
            xPos += GetAutoAdjustUnit(IsAnyControllerKeyDown, xPos);
            yPos += GetAutoAdjustUnit(IsAnyControllerKeyDown, yPos);
            AdjustImage();
        }

        private void AdjustImage()
        {
            Canvas.SetTop(image, originalYPos - yPos / 2);
            Canvas.SetLeft(image, originalXPos + xPos / 2);
        }
    }
}
