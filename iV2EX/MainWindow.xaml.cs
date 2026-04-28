using iV2EX.GetData;
using iV2EX.Util;
using iV2EX.Views;
using Microsoft.UI;
using Microsoft.UI.Input;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Windows.Storage;
using WinRT.Interop;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace iV2EX
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        AppWindow m_AppWindow;

        public MainWindow()
        {
            this.InitializeComponent();

            m_AppWindow = GetAppWindowForCurrentWindow();

            // Check to see if customization is supported.
            // Currently only supported on Windows 11.
            if (AppWindowTitleBar.IsCustomizationSupported())
            {
                var titleBar = m_AppWindow.TitleBar;
                titleBar.ExtendsContentIntoTitleBar = true;
                AppTitleBar.Loaded += AppTitleBar_Loaded;
                AppTitleBar.SizeChanged += AppTitleBar_SizeChanged;

                BackButton.Click += OnBackClicked;
            }
            else
            {
                // Title bar customization using these APIs is currently
                // supported only on Windows 11. In other cases, hide
                // the custom title bar element.
                // AppTitleBar.Visibility = Visibility.Collapsed;
                // TODO Show alternative UI for any functionality in
                // the title bar, such as the back button, if used
            }
        }

        public Button BackButton => AppTitleBarBackButton;

        public void UpdateBackButton()
        {
            if (PageStack.CanGoBack)
            {
                BackButton.Visibility = Visibility.Visible;
                IconColumn.Width = new GridLength(48);
                TitleTextBlock.Margin = new Thickness(4, 0, 0, 0);
            }
            else
            {
                BackButton.Visibility = Visibility.Collapsed;
                IconColumn.Width = new GridLength(0);
                TitleTextBlock.Margin = new Thickness(12, 0, 0, 0);
            }
        }

        private void AppTitleBar_Loaded(object sender, RoutedEventArgs e)
        {
            SetTitleBar(AppTitleBar);
            var localSettings = ApplicationData.Current.LocalSettings;
            var hasCookies = false;
            if (localSettings.Values["Cookies"] != null)
            {
                var cookiesHeader = (string)localSettings.Values["Cookies"];
                var container = ApiClient.Handler.CookieContainer;
                foreach (var item in Regex.Split(cookiesHeader, "; "))
                {
                    var index = item.IndexOf('=');
                    if (index < 0) continue;
                    var name = item.Substring(0, index);
                    var value = item.Substring(index + 1);
                    container.Add(new Uri("https://www.v2ex.com"), new Cookie(name, value));
                }
                hasCookies = true;
            }

            PageFrame.Navigate(hasCookies ? typeof(MainPage) : typeof(UserLoginView));
            UpdateBackButton();

            if (AppWindowTitleBar.IsCustomizationSupported())
            {
                SetDragRegionForCustomTitleBar(m_AppWindow);
            }
        }

        private void OnBackClicked(object sender, RoutedEventArgs e)
        {
            if (PageStack.CanGoBack)
            {
                PageStack.Back();
                UpdateBackButton();
            }
        }

        private void RootGrid_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            var point = e.GetCurrentPoint(RootGrid);
            if (point.Properties.IsXButton1Pressed)
            {
                e.Handled = true;
                if (PageStack.CanGoBack)
                {
                    PageStack.Back();
                    UpdateBackButton();
                }
            }
        }

        private void AppTitleBar_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (AppWindowTitleBar.IsCustomizationSupported()
                && m_AppWindow.TitleBar.ExtendsContentIntoTitleBar)
            {
                // Update drag region if the size of the title bar changes.
                SetDragRegionForCustomTitleBar(m_AppWindow);
            }
        }

        private AppWindow GetAppWindowForCurrentWindow()
        {
            IntPtr hWnd = WindowNative.GetWindowHandle(this);
            WindowId wndId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hWnd);
            return AppWindow.GetFromWindowId(wndId);
        }

        private void SetDragRegionForCustomTitleBar(AppWindow appWindow)
        {
            if (AppWindowTitleBar.IsCustomizationSupported()
                && appWindow.TitleBar.ExtendsContentIntoTitleBar)
            {
                double scaleAdjustment = GetScaleAdjustment();

                RightPaddingColumn.Width = new GridLength(appWindow.TitleBar.RightInset / scaleAdjustment);
                LeftPaddingColumn.Width = new GridLength(appWindow.TitleBar.LeftInset / scaleAdjustment);

                List<Windows.Graphics.RectInt32> dragRectsList = new();

                Windows.Graphics.RectInt32 dragRectL;
                dragRectL.X = (int)((LeftPaddingColumn.ActualWidth + IconColumn.ActualWidth) * scaleAdjustment);
                dragRectL.Y = 0;
                dragRectL.Height = (int)((AppTitleBar.ActualHeight) * scaleAdjustment);
                dragRectL.Width = (int)((TitleColumn.ActualWidth
                                        + DragColumn.ActualWidth) * scaleAdjustment);
                dragRectsList.Add(dragRectL);

                Windows.Graphics.RectInt32[] dragRects = dragRectsList.ToArray();
                appWindow.TitleBar.SetDragRectangles(dragRects);
            }
        }

        [DllImport("Shcore.dll", SetLastError = true)]
        internal static extern int GetDpiForMonitor(IntPtr hmonitor, Monitor_DPI_Type dpiType, out uint dpiX, out uint dpiY);

        internal enum Monitor_DPI_Type : int
        {
            MDT_Effective_DPI = 0,
            MDT_Angular_DPI = 1,
            MDT_Raw_DPI = 2,
            MDT_Default = MDT_Effective_DPI
        }

        private double GetScaleAdjustment()
        {
            IntPtr hWnd = WindowNative.GetWindowHandle(this);
            WindowId wndId = Win32Interop.GetWindowIdFromWindow(hWnd);
            DisplayArea displayArea = DisplayArea.GetFromWindowId(wndId, DisplayAreaFallback.Primary);
            IntPtr hMonitor = Win32Interop.GetMonitorFromDisplayId(displayArea.DisplayId);

            // Get DPI.
            int result = GetDpiForMonitor(hMonitor, Monitor_DPI_Type.MDT_Default, out uint dpiX, out uint _);
            if (result != 0)
            {
                throw new Exception("Could not get DPI for monitor.");
            }

            uint scaleFactorPercent = (uint)(((long)dpiX * 100 + (96 >> 1)) / 96);
            return scaleFactorPercent / 100.0;
        }
    }
}
