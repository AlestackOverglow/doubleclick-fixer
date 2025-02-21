# Mouse Double Click Fixer

A program to fix the problem of double clicks that occur due to mechanical wear of the left mouse button.

Works in the background and blocks unwanted repeated clicks. Takes up minimal space and uses minimal resources (like zero), no admin rights required .

**Note: If it doesn't work, just adjust the threshold in the settings, because different mice require different ones.**
**When launched, the program is automatically minimized to the system tray**

## Features

- Block unwanted double clicks
- Configurable time threshold between clicks
- Work in the background via the system tray
- Automatically save settings
- Start with Windows (disabled by default)

## System requirements

- Windows 7/8/10/11
- .NET Framework 4.8 or higher
- No administrator rights required

## Installation

1. Download the latest version from [Releases](https://github.com/AlestackOverglow/doubleclick-fixer/releases)
2. Make sure you have .NET Framework installed
3. Run `MouseFix.exe`
4. (Optional) Double-click on the tray icon or right-click  → Settings, set the filter threshold that will help your mouse

## Build from source 
   1. Install [.Net SDK](https://dotnet.microsoft.com/download/dotnet?cid=getdotnetcorecli) 
   2.  ```bash
       git clone https://github.com/AlestackOverglow/doubleclick-fixer.git
       cd doubleclick-fixer
       dotnet build
       ```
## Usage

### First launch
- When launched, the program is automatically minimized to the system tray
- The program icon will appear in the tray
- The program immediately starts working with default settings (threshold 50ms)

### Setting
1. Open the settings in one of the following ways:
   - Double-click on the tray icon
   - Right-click on the icon → Settings
2. Set the desired time threshold between clicks:
   - Lower value = softer filtering
   - Higher value = more aggressive click filtering
   - Recommended range: 20-100ms
3. To add to Windows startup:
   -  Right-click on the icon → Run at startup or enable in settings as above
3. Click "Apply" to save the settings
     
### Program management
- **Opening settings**: double-click on the tray icon
- **Exiting the program**: right-click on the icon → Exit
- **Settings are saved** automatically when:
   - Clicking the Apply button in the settings
   - Closing the program
  
## Troubleshooting

### The program does not block unwanted clicks
- Increase the time threshold in the settings
- Try values ​​in the range of 40-100ms

### The program blocks normal clicks
- Reduce the time threshold in the settings
- Try values ​​in the range of 20-80ms

### The program does not start
- Make sure that .NET Framework 4.8 is installed
- Check for write access to the program folder

## Technical details

- The program uses a low-level mouse hook to intercept events
- Settings are saved in the file `mousefix_config.xml`
- Minimum threshold: 10ms
- Maximum threshold: 300ms
