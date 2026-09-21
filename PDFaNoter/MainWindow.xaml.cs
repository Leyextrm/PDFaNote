using System;
using System.Runtime.InteropServices;
using Microsoft.UI.Xaml;

namespace PDFaNoter
{
    public sealed partial class MainWindow : Window
    {
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern bool SetProp(IntPtr hWnd, string lpString, IntPtr hData);

        [DllImport("user32.dll", SetLastError = true, EntryPoint = "SetPropW")]
        private static extern bool SetPropWithAtom(IntPtr hWnd, IntPtr atom, IntPtr hData);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern ushort GlobalAddAtom(string lpString);

        [DllImport("comctl32.dll", SetLastError = true)]
        private static extern bool SetWindowSubclass(IntPtr hWnd, SubclassProc pfnSubclass, UIntPtr uIdSubclass, UIntPtr dwRefData);

        [DllImport("comctl32.dll", SetLastError = true)]
        private static extern IntPtr DefSubclassProc(IntPtr hWnd, uint uMsg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool EnumChildWindows(IntPtr hwndParent, EnumWindowsProc lpEnumFunc, IntPtr lParam);

        private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);
        private delegate IntPtr SubclassProc(IntPtr hWnd, uint uMsg, IntPtr wParam, IntPtr lParam, UIntPtr uIdSubclass, UIntPtr dwRefData);

        private const uint WM_TABLET_QUERYSYSTEMGESTURESTATUS = 0x02CC;

        private const int TABLET_DISABLE_PRESSANDHOLD = 0x00000001;
        private const int TABLET_DISABLE_PENTAPFEEDBACK = 0x00000008;
        private const int TABLET_DISABLE_PENBARRELFEEDBACK = 0x00000010;
        private const int TABLET_DISABLE_TOUCHUIFORCEON = 0x00000100;
        private const int TABLET_DISABLE_TOUCHUIFORCEOFF = 0x00000200;
        private const int TABLET_DISABLE_TOUCHSWITCH = 0x00008000;
        private const int TABLET_DISABLE_FLICKS = 0x00010000;
        private const int TABLET_ENABLE_FLICKSONOFF = 0x00020000;
        private const int TABLET_ENABLE_FLICKLEARNINGMODE = 0x00040000;
        private const int TABLET_DISABLE_SMOOTHSCROLLING = 0x00080000;
        private const int TABLET_DISABLE_FLICKFALLBACKKEYS = 0x00100000;

        private const int ALL_TABLET_DISABLE_FLAGS = 
            TABLET_DISABLE_PRESSANDHOLD |
            TABLET_DISABLE_PENTAPFEEDBACK |
            TABLET_DISABLE_PENBARRELFEEDBACK |
            TABLET_DISABLE_FLICKS |
            TABLET_DISABLE_SMOOTHSCROLLING |
            TABLET_DISABLE_FLICKFALLBACKKEYS;

        private const string TABLET_PROP_NAME = "MicrosoftTabletPenServiceProperty";
        private static readonly SubclassProc _subclassProc = WndProcSubclass;
        private static ushort _tabletAtom = 0;

        private static IntPtr WndProcSubclass(IntPtr hWnd, uint uMsg, IntPtr wParam, IntPtr lParam, UIntPtr uIdSubclass, UIntPtr dwRefData)
        {
            if (uMsg == WM_TABLET_QUERYSYSTEMGESTURESTATUS)
            {
                return new IntPtr(ALL_TABLET_DISABLE_FLAGS);
            }
            return DefSubclassProc(hWnd, uMsg, wParam, lParam);
        }

        private static void ApplyTabletDisables(IntPtr targetHwnd)
        {
            if (targetHwnd == IntPtr.Zero) return;

            SetProp(targetHwnd, TABLET_PROP_NAME, new IntPtr(ALL_TABLET_DISABLE_FLAGS));
            if (_tabletAtom == 0)
            {
                _tabletAtom = GlobalAddAtom(TABLET_PROP_NAME);
            }
            if (_tabletAtom != 0)
            {
                SetPropWithAtom(targetHwnd, (IntPtr)_tabletAtom, new IntPtr(ALL_TABLET_DISABLE_FLAGS));
            }
            SetWindowSubclass(targetHwnd, _subclassProc, (UIntPtr)1001, UIntPtr.Zero);
        }

        public MainPage CurrentMainPage => RootFrame.Content as MainPage;

        public MainWindow()
        {
            InitializeComponent();

            IntPtr hWnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            if (hWnd != IntPtr.Zero)
            {
                ApplyTabletDisables(hWnd);
                EnumChildWindows(hWnd, (childHwnd, lparam) =>
                {
                    ApplyTabletDisables(childHwnd);
                    return true;
                }, IntPtr.Zero);
            }

            this.Activated += (s, e) =>
            {
                if (hWnd != IntPtr.Zero)
                {
                    EnumChildWindows(hWnd, (childHwnd, lparam) =>
                    {
                        ApplyTabletDisables(childHwnd);
                        return true;
                    }, IntPtr.Zero);
                }
            };

            ExtendsContentIntoTitleBar = true;
            SetTitleBar(AppTitleBar);

            string iconPath = System.IO.Path.Combine(System.AppContext.BaseDirectory, "Assets", "AppIcon.ico");
            if (System.IO.File.Exists(iconPath))
            {
                AppWindow.SetIcon(iconPath);
            }
            else
            {
                AppWindow.SetIcon("Assets/AppIcon.ico");
            }

            RootFrame.Navigate(typeof(MainPage));
            AppWindow.Closing += AppWindow_Closing;
        }

        private async void AppWindow_Closing(Microsoft.UI.Windowing.AppWindow sender, Microsoft.UI.Windowing.AppWindowClosingEventArgs args)
        {
            if (RootFrame.Content is MainPage mainPage && mainPage.HasUnsavedChanges())
            {
                args.Cancel = true; // Prevent immediate close
                await mainPage.PromptSaveAndCloseAsync();
            }
        }
    }
}
