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

        private const int USER_MOVE_UNIT = 6;
        private const int ADJUST_MOVE_UNIT = 1;

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
            if (yPos + USER_MOVE_UNIT <= maxYPos) yPos += USER_MOVE_UNIT;
            AdjustImage();
        }

        public void PullDown()
        {
            if (yPos - USER_MOVE_UNIT >= minYPos) yPos -= USER_MOVE_UNIT;
            AdjustImage();
        }

        public void PullRight()
        {
            if (xPos + USER_MOVE_UNIT <= maxXPos) xPos += USER_MOVE_UNIT;
            AdjustImage();
        }

        public void PullLeft()
        {
            if (xPos - USER_MOVE_UNIT >= minXPos) xPos -= USER_MOVE_UNIT;
            AdjustImage();
        }

        public void AutoAdjust()
        {
            if (xPos > 0 && xPos - ADJUST_MOVE_UNIT >= 0) xPos -= ADJUST_MOVE_UNIT;
            if (xPos < 0 && xPos + ADJUST_MOVE_UNIT <= 0) xPos += ADJUST_MOVE_UNIT;

            if (yPos > 0 && yPos - ADJUST_MOVE_UNIT >= 0) yPos -= ADJUST_MOVE_UNIT;
            if (yPos < 0 && yPos + ADJUST_MOVE_UNIT <= 0) yPos += ADJUST_MOVE_UNIT;
            AdjustImage();
        }

        private void AdjustImage()
        {
            Canvas.SetTop(image, originalYPos - yPos / 2);
            Canvas.SetLeft(image, originalXPos + xPos / 2);
        }
    }
}
