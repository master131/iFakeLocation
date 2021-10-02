# iFakeLocation

![](https://i.imgur.com/ELFifkA.png)

## Requirements:
### Windows:
* .NET Framework 4.5 or newer (pre-installed on Windows 8 & Windows 10)  
  https://dotnet.microsoft.com/download/dotnet-framework

* iTunes (Microsoft Store version or Win32/Win64 is fine)
  https://www.apple.com/itunes/download/
  
* Visual C++ Redistributable for Visual Studio 2015  
  https://www.microsoft.com/en-us/download/details.aspx?id=48145
  
### Mac OSX:
* .NET 5.0 Runtime (macOS 10.13 "High Sierra" or newer)
  https://dotnet.microsoft.com/download/dotnet/thank-you/runtime-5.0.10-macos-x64-installer

### Ubuntu:
* .NET 5.0 Runtime (only dotnet-runtime-5.0 package is required)
  https://docs.microsoft.com/en-gb/dotnet/core/install/linux-ubuntu
  
## Download:
See the [Releases](https://github.com/master131/iFakeLocation/releases) page.

## Running:
### Windows:
Run the executable called iFakeLocation.exe.

### Mac OSX
Open the DMG and drag the application to the Desktop or Applications folder. Double-click to run the app.

### Ubuntu
```
chmod +x iFakeLocation.sh
./iFakeLocation.sh
```

## How to make it work on iOS X.X?

If for whatever reason the automatic developer image retrieval doesn't work, you can manually download them to be used in iFakeLocation.
Create a folder called "DeveloperImages" (next to the iFakeLocation executable) and inside that folder make a folder for the iOS version you are running (eg. "12.4", "13.0", etc). Download the matching developer images from the following Github repo and unzip the DeveloperDiskImage.dmg + DeveloperDiskImage.dmg.signature file into the folder you created.

https://github.com/haikieu/xcode-developer-disk-image-all-platforms/tree/master/DiskImages/iPhoneOS.platform/DeviceSupport

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
