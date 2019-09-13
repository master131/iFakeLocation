# iFakeLocation

![](https://i.imgur.com/ELFifkA.png)

## Requirements:
### Windows:
* .NET Framework 4.5 or newer (pre-installed on Windows 8 & Windows 10)  
  https://dotnet.microsoft.com/download/dotnet-framework

* iTunes (Microsoft Store version or Win32/Win64 is fine)  
  https://www.apple.com/itunes/download/
  
### Mac OSX:
* .NET Core 2.2 (macOS 10.12 "Sierra" or newer)  
  https://dotnet.microsoft.com/download/thank-you/dotnet-runtime-2.2.3-macos-x64-installer

### Ubuntu:
* .NET Core 2.2  
  https://dotnet.microsoft.com/download/linux-package-manager/ubuntu16-04/runtime-2.2.3
  
## Download:
See the [Releases](https://github.com/master131/iFakeLocation/releases) page.

## Running:
### Windows:
Run the executable called iFakeLocation.exe.

### Mac OSX
Open the DMG and drag the application to the Desktop or Applications folder. Double-click to run the app.

### Ubuntu
```
chmod +x iFakeLocation
./iFakeLocation
```

## How to make it work on iOS X.X?

Create a folder called "DeveloperImages" (next to the iFakeLocation executable) and inside that folder make a folder for the iOS version you are running (eg. "12.4", "13.0", etc). Download the matching developer images below and place them in the corresponding folder.

* iOS 12.3/12.4 Developer Images:

[DeveloperDiskImage.dmg](https://github.com/xushuduo/Xcode-iOS-Developer-Disk-Image/raw/master/Developer%20Disk%20Image/12.3%20(16F148)/DeveloperDiskImage.dmg)

[DeveloperDiskImage.dmg.signature](https://github.com/xushuduo/Xcode-iOS-Developer-Disk-Image/raw/master/Developer%20Disk%20Image/12.3%20(16F148)/DeveloperDiskImage.dmg.signature)

* iOS 13.0 Developer Images:

[DeveloperDiskImage.dmg](https://github.com/xushuduo/Xcode-iOS-Developer-Disk-Image/raw/master/Developer%20Disk%20Image/13.0%20(17A5565b)/DeveloperDiskImage.dmg)

[DeveloperDiskImage.dmg.signature](https://github.com/xushuduo/Xcode-iOS-Developer-Disk-Image/raw/master/Developer%20Disk%20Image/13.0%20(17A5565b)/DeveloperDiskImage.dmg.signature)

ie.

DeveloperImages\12.4\DeveloperDiskImage.dmg

DeveloperImages\12.4\DeveloperDiskImage.dmg.signature

## How to use:
* Connect your iDevice to your computer. Click the "Refresh" button and select your iDevice from the list.

* Enter the desired location (ie. Sydney NSW) in the box and hit "Search" (try to be
  more specific if you are getting strange results).

  You can also manually place a pin on the map by double-clicking anywhere.

* Click "Set Fake Location". If it is the first time doing this the tool
  needs to download some files to enable Developer Mode on your iDevice.

* Confirm your fake location using Apple Maps, Google Maps, etc. To stop the fake location,
  click "Stop Fake Location". If your device is still stuck at the faked location
  turn Location Services off and on in Settings > Privacy.

* Your device will also have a Developer menu now shown in Settings. You can get rid of it 
  by restarting your iDevice.

## Help:
Q: My device doesn't show up on the list?  
A: Ensure that it is plugged in, you have trusted your PC and that the device is visible on iTunes.

## Special Thanks:
* [idevicelocation by JonGabilondoAngulo](https://github.com/JonGabilondoAngulo/idevicelocation)
* [Xcode-iOS-Developer-Disk-Image by xushuduo](https://github.com/xushuduo/Xcode-iOS-Developer-Disk-Image/)
