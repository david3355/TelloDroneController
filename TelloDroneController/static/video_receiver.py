from djitellopy import Tello
import cv2
import pygame
import numpy as np
import time
import sys

# Frames per second of the pygame window display
FPS = 25


class FrontEnd(object):

    def __init__(self, window_width, window_height):
        # Init pygame
        pygame.init()

        # Creat pygame window
        self.screen = pygame.display.set_mode([window_width, window_height])  # [960, 720]

        # Init Tello object that interacts with the Tello drone
        self.tello = Tello()

        # create update timer
        #pygame.time.set_timer(pygame.USEREVENT + 1, 50)

    def run(self):
        pygame.display.set_caption("Starting frame reading...")
        frame_read = self.tello.get_frame_read()
        pygame.display.set_caption("Receiving Tello video stream")

        should_stop = False
        while not should_stop:

            for event in pygame.event.get():
                if event.type == pygame.QUIT:
                    should_stop = True
                elif event.type == pygame.KEYDOWN:
                    if event.key == pygame.K_ESCAPE:
                        should_stop = True

            if frame_read.stopped:
                frame_read.stop()
                break

            self.screen.fill([0, 0, 0])
            frame = cv2.cvtColor(frame_read.frame, cv2.COLOR_BGR2RGB)
            frame = np.rot90(frame)
            frame = np.flipud(frame)
            frame = pygame.surfarray.make_surface(frame)
            self.screen.blit(frame, (0, 0))
            pygame.display.update()

            time.sleep(1 / FPS)

        # Call it always before finishing. To deallocate resources.
        self.tello.end()


def main():
    try:
        w = 640
        h = 480
        if len(sys.argv) == 3:
            w = int(sys.argv[1])
            h = int(sys.argv[2])
        frontend = FrontEnd(w, h)
        frontend.run()
    except:
        pass


if __name__ == '__main__':
    main()
