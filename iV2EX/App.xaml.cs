using System;
using System.Diagnostics;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;
using Windows.Storage;
using Windows.UI;
using Windows.UI.ViewManagement;
using iV2EX.GetData;
using iV2EX.Views;
using Microsoft.Toolkit.Uwp.UI;
using MyToolkit.Paging;

namespace iV2EX
{
    /// <summary>
    ///     提供特定于应用程序的行为，以补充默认的应用程序类。
    /// </summary>
    public sealed partial class App
    {
        public App()
        {
            InitializeComponent();

#if DEBUG
            if (Debugger.IsAttached)
                DebugSettings.EnableFrameRateCounter = true;
#endif
        }

        public override async Task<Type> GetStartPageTypeAsync()
        {
            var localSettings = ApplicationData.Current.LocalSettings;
            if (localSettings.Values["Cookies"] != null)
            {
                var cookiesHeader = (string) localSettings.Values["Cookies"];
                var container = ApiClient.Client.ApiConfig.HttpClient.Handler.CookieContainer;
                foreach (var item in Regex.Split(cookiesHeader, "; "))
                {
                    var index = item.IndexOf('=');
                    if (index < 0) continue;
                    var name = item.Substring(0, index);
                    var value = item.Substring(index + 1);
                    container.Add(new Uri("https://www.v2ex.com"), new Cookie(name, value));
                }
            }

            return await V2ExCommand.IsLoginAsync() ? typeof(MainPage) : typeof(UserLoginView);
        }

        public override Task OnInitializedAsync(MtFrame frame, ApplicationExecutionState e)
        {
            var titleBar = ApplicationView.GetForCurrentView().TitleBar;
            titleBar.BackgroundColor = Color.FromArgb(255, 63, 81, 181);
            titleBar.InactiveBackgroundColor = Color.FromArgb(255, 63, 81, 181);
            titleBar.ButtonForegroundColor = Colors.White;
            titleBar.ButtonBackgroundColor = Color.FromArgb(255, 63, 81, 181);
            titleBar.ButtonInactiveBackgroundColor = Color.FromArgb(255, 63, 81, 181);
            titleBar.ForegroundColor = Colors.White;
            ImageCache.Instance.CacheDuration = TimeSpan.FromDays(7);
            ImageCache.Instance.InitializeAsync(ApplicationData.Current.TemporaryFolder,
                "CachePics").Wait();
            // TODO: Called when the app is started (not resumed)
            return null;
        }
    }
}