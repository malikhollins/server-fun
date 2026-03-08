from adb_shell.adb_device import AdbDeviceTcp
from input import Input, ScrollDirection
import subprocess
import time

input = Input()
input.connect()

bluestackpath = "C:\\Program Files\\BlueStacks_nxt\\HD-Player.exe"
bluestack = subprocess.Popen(bluestackpath)
time.sleep(10) # Wait for BlueStacks to start
device = AdbDeviceTcp("127.0.0.1", 5555)
device.connect()

device.shell("am start -W com.zhiliaoapp.musically/com.ss.android.ugc.aweme.splash.SplashActivity")

while ( True ):
    direction = input.record_input()
    if direction== ScrollDirection.SCROLL_UP:
        device.shell("input swipe 500 1000 500 500")
        print("Scroll up Detected")
    if direction == ScrollDirection.SCROLL_DOWN:
        device.shell("input swipe 500 500 500 1000")
        print("Scroll down Detected")
    if direction == ScrollDirection.TAP:
        device.shell("input tap 500 500")
        print("Tap Detected")
    if  direction == ScrollDirection.QUIT:
        break

bluestack.kill()
