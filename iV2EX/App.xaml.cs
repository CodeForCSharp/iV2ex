using System;
using System.Diagnostics;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;
using Windows.Storage;
using Windows.UI;
using Windows.UI.ViewManagement;
using AngleSharp.Parser.Html;
using iV2EX.GetData;
using iV2EX.Views;
using Microsoft.Toolkit.Uwp.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Controls;
using Windows.ApplicationModel;
using PagingEx;
using System.Reactive.Linq;
using System.Reactive.Concurrency;

namespace iV2EX
{
    //    /// <summary>
    //    ///     提供特定于应用程序的行为，以补充默认的应用程序类。
    //    /// </summary>
    //    public sealed partial class App
    //    {
    //        public App()
    //        {
    //            InitializeComponent();

    //#if DEBUG
    //            if (Debugger.IsAttached)
    //                DebugSettings.EnableFrameRateCounter = true;
    //#endif
    //        }

    //        public override async Task<Type> GetStartPageTypeAsync()
    //        {
    //            var localSettings = ApplicationData.Current.LocalSettings;
    //            if (localSettings.Values["Cookies"] != null)
    //            {
    //                var cookiesHeader = (string) localSettings.Values["Cookies"];
    //                var container = ApiClient.Handler.CookieContainer;
    //                foreach (var item in Regex.Split(cookiesHeader, "; "))
    //                {
    //                    var index = item.IndexOf('=');
    //                    if (index < 0) continue;
    //                    var name = item.Substring(0, index);
    //                    var value = item.Substring(index + 1);
    //                    container.Add(new Uri("https://www.v2ex.com"), new Cookie(name, value));
    //                }
    //            }

    //            try
    //            {
    //                var content = await ApiClient.GetMainPage();
    //                var b = new HtmlParser().Parse(content).GetElementById("Top").TextContent.Contains("登出");
    //                return b? typeof(MainPage) : typeof(UserLoginView);
    //            }
    //            catch
    //            {
    //                return typeof(UserLoginView);
    //            }
    //        }

    //        public override Task OnInitializedAsync(MtFrame frame, ApplicationExecutionState e)
    //        {
    //            var titleBar = ApplicationView.GetForCurrentView().TitleBar;
    //            titleBar.BackgroundColor = Color.FromArgb(255, 63, 81, 181);
    //            titleBar.InactiveBackgroundColor = Color.FromArgb(255, 63, 81, 181);
    //            titleBar.ButtonForegroundColor = Colors.White;
    //            titleBar.ButtonBackgroundColor = Color.FromArgb(255, 63, 81, 181);
    //            titleBar.ButtonInactiveBackgroundColor = Color.FromArgb(255, 63, 81, 181);
    //            titleBar.ForegroundColor = Colors.White;
    //            ImageCache.Instance.CacheDuration = TimeSpan.FromDays(7);
    //            ImageCache.Instance.InitializeAsync(ApplicationData.Current.TemporaryFolder,
    //                "CachePics").Wait();
    //            // TODO: Called when the app is started (not resumed)
    //            return null;
    //        }
    //    }
    /// <summary>
    /// 提供特定于应用程序的行为，以补充默认的应用程序类。
    /// </summary>
    sealed partial class App : Application
    {
        /// <summary>
        /// 初始化单一实例应用程序对象。这是执行的创作代码的第一行，
        /// 已执行，逻辑上等同于 main() 或 WinMain()。
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;
        }

        /// <summary>
        /// 在应用程序由最终用户正常启动时进行调用。
        /// 将在启动应用程序以打开特定文件等情况下使用。
        /// </summary>
        /// <param name="e">有关启动请求和过程的详细信息。</param>
        protected override async void OnLaunched(LaunchActivatedEventArgs e)
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
            var localSettings = ApplicationData.Current.LocalSettings;
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
            }
            var rootFrame = Window.Current.Content as ActivityContainer;

            // 不要在窗口已包含内容时重复应用程序初始化，
            // 只需确保窗口处于活动状态
            if (rootFrame == null)
            {
                // 创建要充当导航上下文的框架，并导航到第一页
                rootFrame = new ActivityContainer();
                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: 从之前挂起的应用程序加载状态
                }

                // 将框架放在当前窗口中
                Window.Current.Content = rootFrame;
            }

            if (e.PrelaunchActivated == false)
            {
                if (rootFrame.Content == null)
                {
                    // 当导航堆栈尚未还原时，导航到第一页，
                    // 并通过将所需信息作为导航参数传入来配置
                    // 参数
                    try
                    {
                        var html = await ApiClient.GetMainPage();
                        var r = new HtmlParser().Parse(html).GetElementById("Top").TextContent.Contains("登出");
                        rootFrame.Navigate(r ? typeof(MainPage) : typeof(UserLoginView));
                    }
                    catch(Exception ex)
                    {
                        rootFrame.Navigate(typeof(UserLoginView));
                    }
                }
                // 确保当前窗口处于活动状态
                Window.Current.Activate();
            }
        }

        /// <summary>
        /// 导航到特定页失败时调用
        /// </summary>
        ///<param name="sender">导航失败的框架</param>
        ///<param name="e">有关导航失败的详细信息</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        /// 在将要挂起应用程序执行时调用。  在不知道应用程序
        /// 无需知道应用程序会被终止还是会恢复，
        /// 并让内存内容保持不变。
        /// </summary>
        /// <param name="sender">挂起的请求的源。</param>
        /// <param name="e">有关挂起请求的详细信息。</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: 保存应用程序状态并停止任何后台活动
            deferral.Complete();
        }
    }
}