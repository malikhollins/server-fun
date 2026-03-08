import subprocess
import hid
import os

# Wii Remote HID IDs
VENDOR_ID = 0x057e
PRODUCT_ID = 0x0306

class ScrollDirection:
    SCROLL_UP = "scroll_up"
    TAP = "tap"
    SCROLL_DOWN = "scroll_down"
    QUIT = "quit"

class Input:
    wiimote = None

    def __init__(self):
        return

    def connect(self):
        current_dir = os.path.dirname(os.path.abspath(__file__))
        wiipair_path = os.path.join(current_dir, "tmp/WiiPair.exe")
        process = subprocess.Popen([], executable=wiipair_path)
        process.wait()
        process.kill()

        self.wiimote = hid.device()
        self.wiimote.open(VENDOR_ID, PRODUCT_ID) 
        self.wiimote.write([0x11, 0x10])

    """
    This function will continuously listen for user input and return the corresponding 
    command when a valid key is pressed.
    """
    def record_input(self) -> str:
        try:
            # Read reports; adjust size/timeouts to match your environment
            report = self.wiimote.read(22, 60000)
            if report:
                return self.read_report(report)
            else:
                if len(report) == 0:
                    print("Wii Remote disconnected (No data received).")
                    if self.wiimote:
                        self.wiimote.close()
                    return ScrollDirection.QUIT
        except OSError:
            print("Wii Remote disconnected (Power Off or Out of Range).")
        except KeyboardInterrupt:
            print("Interrupted by user")
        except Exception as e:
            print(f"Fatal error: {e}")
        
    def read_report( self, report ) -> str:
        print(f"Raw Report: {report}")
        if report[1] & 0x08:
            return ScrollDirection.SCROLL_UP
        elif report[1] & 0x04:
            return ScrollDirection.SCROLL_DOWN
        elif report[2] & 0x08:
            return ScrollDirection.TAP
