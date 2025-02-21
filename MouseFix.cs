using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using System.Xml.Serialization;
using System.Drawing;

public class MouseFix
{
    private const int WH_MOUSE_LL = 14;
    private const int WM_LBUTTONDOWN = 0x0201;
    private const int WM_LBUTTONUP = 0x0202;
    private const string CONFIG_FILE = "mousefix_config.xml";
    private const int DEFAULT_THRESHOLD = 30; // Default threshold in milliseconds
    private const int DEBOUNCE_TIME = 10; // Minimum time between clicks in milliseconds
    private const int RESET_TIMER_INTERVAL = 100; // Reset timer interval in milliseconds
    
    private static LowLevelMouseProc _proc = HookCallback;
    private static IntPtr _hookID = IntPtr.Zero;
    private static long _lastClickTime = 0;
    private static int _threshold = DEFAULT_THRESHOLD;
    private static NotifyIcon trayIcon;
    private static bool _isBlocking = false;
    private static int _clickCount = 0;
    private static Timer _clickResetTimer;

    [Serializable]
    public class Config
    {
        public int Threshold { get; set; }
        public bool AutoStart { get; set; }
    }

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelMouseProc lpfn, IntPtr hMod, uint dwThreadId);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool UnhookWindowsHookEx(IntPtr hhk);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr GetModuleHandle(string lpModuleName);

    private delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);

    private static void Initialize()
    {
        LoadConfig();
        InitializeTrayIcon();
        _hookID = SetHook(_proc);
        
        // Initialize the timer
        _clickResetTimer = new Timer();
        _clickResetTimer.Interval = RESET_TIMER_INTERVAL;
        _clickResetTimer.Tick += (s, e) => 
        {
            _clickCount = 0;
            _isBlocking = false;
            _clickResetTimer.Stop();
        };
    }

    private static bool GetAutoStartEnabled()
    {
        try
        {
            using (var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", false))
            {
                return key?.GetValue("MouseFix") != null;
            }
        }
        catch
        {
            return false;
        }
    }

    private static void SetAutoStart(bool enable)
    {
        try
        {
            using (var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true))
            {
                if (enable)
                {
                    key?.SetValue("MouseFix", Application.ExecutablePath);
                }
                else
                {
                    key?.DeleteValue("MouseFix", false);
                }
            }
        }
        catch
        {
            // Ignore registry access errors
        }
    }

    private static void LoadConfig()
    {
        try
        {
            if (File.Exists(CONFIG_FILE))
            {
                var serializer = new XmlSerializer(typeof(Config));
                using (var reader = new StreamReader(CONFIG_FILE))
                {
                    var config = (Config)serializer.Deserialize(reader);
                    _threshold = config.Threshold;
                    SetAutoStart(config.AutoStart);
                }
            }
        }
        catch
        {
            _threshold = DEFAULT_THRESHOLD;
        }
    }

    private static void SaveConfig()
    {
        try
        {
            var config = new Config 
            { 
                Threshold = _threshold,
                AutoStart = GetAutoStartEnabled()
            };
            var serializer = new XmlSerializer(typeof(Config));
            using (var writer = new StreamWriter(CONFIG_FILE))
            {
                serializer.Serialize(writer, config);
            }
        }
        catch
        {
            // Just ignore errors
        }
    }

    private static void ShowSettingsForm()
    {
        using (var settingsForm = new Form())
        {
            settingsForm.Text = "Mouse Fix Settings";
            settingsForm.Size = new System.Drawing.Size(280, 150);
            settingsForm.FormBorderStyle = FormBorderStyle.FixedDialog;
            settingsForm.MaximizeBox = false;
            settingsForm.MinimizeBox = false;
            settingsForm.StartPosition = FormStartPosition.CenterScreen;

            var label = new Label
            {
                Text = "Threshold (ms):",
                Location = new System.Drawing.Point(20, 17),
                AutoSize = true
            };

            var thresholdInput = new NumericUpDown
            {
                Location = new System.Drawing.Point(100, 15),
                Minimum = 10,
                Maximum = 300,
                Value = _threshold,
                Width = 80
            };

            var autoStartCheckbox = new CheckBox
            {
                Text = "Run at Windows startup",
                Location = new System.Drawing.Point(20, 45),
                AutoSize = true,
                Checked = GetAutoStartEnabled()
            };

            var applyButton = new Button
            {
                Text = "Apply",
                Location = new System.Drawing.Point(100, 70),
                Size = new System.Drawing.Size(80, 25),
                DialogResult = DialogResult.OK
            };

            var authorLabel = new Label
            {
                Text = "Author: AlestackOverglow",
                Location = new System.Drawing.Point(20, 100),
                AutoSize = true,
                ForeColor = System.Drawing.Color.Blue,
                Cursor = Cursors.Hand
            };

            applyButton.Click += (s, e) =>
            {
                _threshold = (int)thresholdInput.Value;
                SetAutoStart(autoStartCheckbox.Checked);
                SaveConfig();
                settingsForm.Close();
            };

            authorLabel.Click += (s, e) =>
            {
                System.Diagnostics.Process.Start(new ProcessStartInfo
                {
                    FileName = "https://alestackoverglow.github.io/",
                    UseShellExecute = true
                });
            };

            settingsForm.Controls.AddRange(new Control[] { 
                label, 
                thresholdInput, 
                autoStartCheckbox,
                applyButton, 
                authorLabel 
            });
            settingsForm.ShowDialog();
        }
    }

    private static void InitializeTrayIcon()
    {
        trayIcon = new NotifyIcon();
        try 
        {
            trayIcon.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
        }
        catch 
        {
            trayIcon.Icon = SystemIcons.Application;
        }
        trayIcon.Text = "Mouse Double Click Fixer";
        
        var contextMenu = new ContextMenuStrip();
        
        var settingsItem = new ToolStripMenuItem("Settings");
        settingsItem.Click += (s, e) => ShowSettingsForm();
        
        var autoStartItem = new ToolStripMenuItem("Run at startup");
        autoStartItem.Checked = GetAutoStartEnabled();
        autoStartItem.Click += (s, e) =>
        {
            autoStartItem.Checked = !autoStartItem.Checked;
            SetAutoStart(autoStartItem.Checked);
            SaveConfig();
        };
        
        var separator = new ToolStripSeparator();
        
        var exitItem = new ToolStripMenuItem("Exit");
        exitItem.Click += (s, e) => 
        {
            SaveConfig();
            trayIcon.Visible = false;
            trayIcon.Dispose();
            UnhookWindowsHookEx(_hookID);
            Application.Exit();
        };
        
        contextMenu.Items.AddRange(new ToolStripItem[] { 
            settingsItem,
            autoStartItem,
            separator,
            exitItem 
        });
        
        trayIcon.ContextMenuStrip = contextMenu;
        trayIcon.Visible = true;
        
        trayIcon.DoubleClick += (s, e) => ShowSettingsForm();
    }

    private static IntPtr SetHook(LowLevelMouseProc proc)
    {
        using (Process curProcess = Process.GetCurrentProcess())
        using (ProcessModule curModule = curProcess.MainModule)
        {
            return SetWindowsHookEx(WH_MOUSE_LL, proc,
                GetModuleHandle(curModule.ModuleName), 0);
        }
    }

    private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0)
        {
            if (wParam == (IntPtr)WM_LBUTTONDOWN)
            {
                long currentTime = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
                long timeDiff = currentTime - _lastClickTime;

                // Ignore clicks that are too close together (debouncing)
                if (timeDiff < DEBOUNCE_TIME)
                {
                    return (IntPtr)1;
                }

                if (timeDiff < _threshold)
                {
                    _clickCount++;
                    if (_clickCount > 1)
                    {
                        _isBlocking = true;
                        return (IntPtr)1;
                    }
                }
                else
                {
                    _clickCount = 1;
                }

                _lastClickTime = currentTime;
                if (_clickResetTimer != null)
                {
                    _clickResetTimer.Start();
                }
            }
            else if (wParam == (IntPtr)WM_LBUTTONUP)
            {
                if (_isBlocking)
                {
                    _isBlocking = false;
                    return (IntPtr)1;
                }
            }
        }

        return CallNextHookEx(_hookID, nCode, wParam, lParam);
    }

    [STAThread]
    public static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Initialize();
        Application.Run();
    }
} 