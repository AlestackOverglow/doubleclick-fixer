using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using System.Xml.Serialization;

public class MouseFix
{
    private const int WH_MOUSE_LL = 14;
    private const int WM_LBUTTONDOWN = 0x0201;
    private const int WM_LBUTTONUP = 0x0202;
    private const string CONFIG_FILE = "mousefix_config.xml";
    
    private static LowLevelMouseProc _proc = HookCallback;
    private static IntPtr _hookID = IntPtr.Zero;
    private static long _lastClickTime = 0;
    private static int _threshold = 50;
    private static NotifyIcon trayIcon;
    private static bool _isBlocking = false;
    private static int _clickCount = 0;
    private static Timer _clickResetTimer;

    [Serializable]
    public class Config
    {
        public int Threshold { get; set; }
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
        _clickResetTimer.Interval = 200; // Reset interval in milliseconds
        _clickResetTimer.Tick += (s, e) => 
        {
            _clickCount = 0; // Reset click count
            _clickResetTimer.Stop(); // Stop the timer
        };
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
                }
            }
        }
        catch
        {
            // Use default threshold if there is an error
            _threshold = 50;
        }
    }

    private static void SaveConfig()
    {
        try
        {
            var config = new Config { Threshold = _threshold };
            var serializer = new XmlSerializer(typeof(Config));
            using (var writer = new StreamWriter(CONFIG_FILE))
            {
                serializer.Serialize(writer, config);
            }
        }
        catch
        {
            // Just ignore errors lol
        }
    }

    private static void ShowSettingsForm()
    {
        using (var settingsForm = new Form())
        {
            settingsForm.Text = "Mouse Fix Settings";
            settingsForm.Size = new System.Drawing.Size(300, 150);
            settingsForm.FormBorderStyle = FormBorderStyle.FixedDialog;
            settingsForm.MaximizeBox = false;
            settingsForm.MinimizeBox = false;
            settingsForm.StartPosition = FormStartPosition.CenterScreen;

            var label = new Label
            {
                Text = "Threshold (ms):",
                Location = new System.Drawing.Point(20, 20),
                AutoSize = true
            };

            var thresholdInput = new NumericUpDown
            {
                Location = new System.Drawing.Point(120, 18),
                Minimum = 10,
                Maximum = 300,
                Value = _threshold,
                Width = 60
            };

            var applyButton = new Button
            {
                Text = "Apply",
                Location = new System.Drawing.Point(100, 60),
                DialogResult = DialogResult.OK
            };

            applyButton.Click += (s, e) =>
            {
                _threshold = (int)thresholdInput.Value;
                SaveConfig();
                settingsForm.Close();
            };

            settingsForm.Controls.AddRange(new Control[] { label, thresholdInput, applyButton });
            settingsForm.ShowDialog();
        }
    }

    private static void InitializeTrayIcon()
    {
        trayIcon = new NotifyIcon();
        trayIcon.Icon = System.Drawing.SystemIcons.Application;
        trayIcon.Text = "Mouse Double Click Fixer";
        
        var contextMenu = new ContextMenuStrip();
        
        var settingsItem = new ToolStripMenuItem("Settings");
        settingsItem.Click += (s, e) => ShowSettingsForm();
        
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
                    _clickCount = 1; // Reset count if time exceeds threshold
                }

                _lastClickTime = currentTime;
                _clickResetTimer.Start(); // Start or reset the timer
                _isBlocking = false;
            }
            else if (wParam == (IntPtr)WM_LBUTTONUP && _isBlocking)
            {
                return (IntPtr)1;
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