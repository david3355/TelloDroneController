# coding=utf-8
import logging
import socket
import threading
from threading import Thread

import cv2
import time



class Tello:
    """Python wrapper to interact with the Ryze Tello drone using the official Tello api.
    Tello API documentation:
    https://dl-cdn.ryzerobotics.com/downloads/tello/20180910/Tello%20SDK%20Documentation%20EN_1.3.pdf
    """
    # Send and receive commands, client socket

    HANDLER = logging.StreamHandler()
    FORMATTER = logging.Formatter('%(filename)s - %(lineno)d - %(message)s')
    HANDLER.setFormatter(FORMATTER)

    LOGGER = logging.getLogger('djitellopy')

    LOGGER.addHandler(HANDLER)
    LOGGER.setLevel(logging.INFO)
    # use logging.getLogger('djitellopy').setLevel(logging.<LEVEL>) in YOUR CODE
    # to only receive logs of the desired level and higher

    # Video stream, server socket
    VS_UDP_IP = '0.0.0.0'
    VS_UDP_PORT = 11111

    # VideoCapture object
    cap = None
    background_frame_read = None

    stream_on = False

    def __init__(self):

        self.stream_on = False


    def get_udp_video_address(self):
        return 'udp://@' + self.VS_UDP_IP + ':' + str(self.VS_UDP_PORT)  # + '?overrun_nonfatal=1&fifo_size=5000'

    def get_video_capture(self):
        """Get the VideoCapture object from the camera drone
        Returns:
            VideoCapture
        """

        if self.cap is None:
            self.cap = cv2.VideoCapture(self.get_udp_video_address())

        if not self.cap.isOpened():
            self.cap.open(self.get_udp_video_address())

        return self.cap

    def get_frame_read(self):
        """Get the BackgroundFrameRead object from the camera drone. Then, you just need to call
        backgroundFrameRead.frame to get the actual frame received by the drone.
        Returns:
            BackgroundFrameRead
        """
        if self.background_frame_read is None:
            self.background_frame_read = BackgroundFrameRead(self, self.get_udp_video_address()).start()
        return self.background_frame_read

    def streamon(self):
        """Set video stream on. If the response is 'Unknown command' means you have to update the Tello firmware. That
        can be done through the Tello app.
        Returns:
            bool: True for successful, False for unsuccessful
        """
        self.stream_on = True

    def streamoff(self):
        """Set video stream off
        Returns:
            bool: True for successful, False for unsuccessful
        """
        self.stream_on = False

    def end(self):
        """Call this method when you want to end the tello object"""
        try:
            if self.background_frame_read is not None:
                self.background_frame_read.stop()
            if self.cap is not None:
                self.cap.release()
        except:
            pass

    def __del__(self):
        self.end()


class BackgroundFrameRead:
    """
    This class read frames from a VideoCapture in background. Then, just call backgroundFrameRead.frame to get the
    actual one.
    """

    def __init__(self, tello, address):
        tello.cap = cv2.VideoCapture(address)
        self.cap = tello.cap

        if not self.cap.isOpened():
            self.cap.open(address)

        self.grabbed, self.frame = self.cap.read()
        self.stopped = False

    def start(self):
        Thread(target=self.update_frame, args=()).start()
        return self

    def update_frame(self):
        while not self.stopped:
            if not self.grabbed or not self.cap.isOpened():
                self.stop()
            else:
                (self.grabbed, self.frame) = self.cap.read()

    def stop(self):
        self.stopped = True
