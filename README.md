# Mouse Double Click Fixer

A program to fix the problem of double clicks when the mouse is worn out. 
Works in the background and blocks unwanted repeated clicks that occur due to mechanical wear of the left mouse button.
Takes up minimal space and uses minimal resources (like zero)

## Features

- Block unwanted double clicks
- Configurable time threshold between clicks
- Work in the background via the system tray
- Automatically save settings
- Start with Windows (must be added manually)

## System requirements

- Windows 7/8/10/11
- .NET Framework 4.8 or higher
- No administrator rights required

## Installation

1. Download the latest version of the program
2. Unzip the files to a convenient location
3. Run `MouseFix.exe`
4. (Optional) Double-click on the tray icon or right-click  → Settings, set the filter threshold that will help your mouse

## Usage

### First launch
- When launched, the program is automatically minimized to the system tray
- The program icon will appear in the tray
- The program immediately starts working with default settings (threshold 30ms)

### Setting
1. Open the settings in one of the following ways:
   - Double-click on the tray icon
   - Right-click on the icon → Settings
2. Set the desired time threshold between clicks:
   - Lower value = more aggressive click filtering
   - Higher value = softer filtering
   - Recommended range: 20-50ms
3. Click "Apply" to save the settings

### Program management
- **Opening settings**: double-click on the tray icon
- **Exiting the program**: right-click on the icon → Exit
- **Settings are saved** automatically when:
- Clicking the Apply button in the settings
- Closing the program
  
### Autostart
To add to Windows startup:
1. Create a shortcut `MouseFix.exe`
2. Press Win + R, enter `shell:startup`
3. Copy the shortcut to the folder that opens

## Troubleshooting

### The program does not block unwanted clicks
- Reduce the time threshold in the settings
- Try values ​​in the range of 20-30ms

### The program blocks normal clicks
- Increase the time threshold in the settings
- Try values ​​in the range of 40-50ms

### The program does not start
- Make sure that .NET Framework 4.8 is installed
- Check for write access to the program folder
- 
## Build from source 
   - Install [.Net SDK](https://dotnet.microsoft.com/download/dotnet?cid=getdotnetcorecli)
   - Open command line in project folder or
     ```bash
      cd your/project/folder
     ```
   - Type:
     ```bash
      dotnet build
      ```
     
## Technical details

- The program uses a low-level mouse hook to intercept events
- Settings are saved in the file `mousefix_config.xml`
- Minimum threshold: 10ms
- Maximum threshold: 200ms
