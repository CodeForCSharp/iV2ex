using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using AngleSharp.Html.Parser;
using Windows.Storage;
using Windows.System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using iV2EX.GetData;
using iV2EX.Model;
using iV2EX.Util;

namespace iV2EX.Views
{
    public sealed partial class UserLoginView
    {
        private LoginModel _data;
        private bool _isLoadingCaptcha;
        private bool _isSubmitting;
        private readonly List<Control> _controls;

        public UserLoginView()
        {
            InitializeComponent();
            _controls = new List<Control> { TbUsername, TbPassword, TbCaptcha, RefreshCaptchaButton, BtnLogin };
            UserLoginPage.Loaded += UserLoginPage_Loaded;
        }

        private async void UserLoginPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (_data != null) return;
            await RefreshCaptchaAsync(showErrorToast: true);
        }

        private async void RefreshCaptchaButton_Click(object sender, RoutedEventArgs e)
        {
            await RefreshCaptchaAsync(showErrorToast: true);
        }

        private async void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            await SubmitLoginAsync();
        }

        private async void Input_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Enter && !_isSubmitting)
            {
                e.Handled = true;
                await SubmitLoginAsync();
            }
        }

        private static async Task<LoginModel> FetchLoginDataAsync()
        {
            var html = await ApiClient.GetSignInInformation();
            var form = new HtmlParser().ParseDocument(html).QuerySelector("form[action='/signin']");
            var inputs = form?.QuerySelectorAll("input");
            if (inputs == null || inputs.Length < 4)
                throw new InvalidOperationException("无法解析登录表单");

            var once = inputs[3].GetAttribute("value");
            return new LoginModel
            {
                UName = inputs[0].GetAttribute("name"),
                PName = inputs[1].GetAttribute("name"),
                CName = inputs[2].GetAttribute("name"),
                Once = once,
                CImage = $"https://www.v2ex.com/_captcha?once={once}"
            };
        }

        private async Task RefreshCaptchaAsync(bool showErrorToast)
        {
            if (_isLoadingCaptcha) return;
            _isLoadingCaptcha = true;
            RefreshCaptchaButton.IsEnabled = false;
            CaptchaLoadingRing.IsActive = true;
            CaptchaLoadingRing.Visibility = Microsoft.UI.Xaml.Visibility.Visible;

            try
            {
                _data = await AsyncHelper.RetryAsync(FetchLoginDataAsync, 3);
                CaptchaImage.Source = await GetBitmapFromUrl.GetBitmapFromStream(_data.CImage);
            }
            catch
            {
                _data = null;
                CaptchaImage.Source = null;
                if (showErrorToast)
                    Toast.ShowTips("验证码加载失败，请点击重试");
            }
            finally
            {
                _isLoadingCaptcha = false;
                RefreshCaptchaButton.IsEnabled = !_isSubmitting;
                CaptchaLoadingRing.IsActive = false;
                CaptchaLoadingRing.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
            }
        }

        private async Task SubmitLoginAsync()
        {
            if (_isSubmitting) return;
            if (string.IsNullOrWhiteSpace(TbUsername.Text)) { Toast.ShowTips("账号不能为空"); return; }
            if (string.IsNullOrEmpty(TbPassword.Password)) { Toast.ShowTips("密码不能为空"); return; }
            if (string.IsNullOrWhiteSpace(TbCaptcha.Text)) { Toast.ShowTips("验证码不能为空"); return; }

            if (_data == null)
            {
                await RefreshCaptchaAsync(showErrorToast: true);
                if (_data == null) return;
            }

            _isSubmitting = true;
            _controls.ForEach(c => c.IsEnabled = false);
            LoginRing.IsActive = true;
            LoginRing.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
            BtnLoginText.Text = "登录中...";

            try
            {
                var parameters = new Dictionary<string, string>
                {
                    { "next", "/" },
                    { _data.UName, TbUsername.Text.Trim() },
                    { "once", _data.Once },
                    { _data.PName, TbPassword.Password },
                    { _data.CName, TbCaptcha.Text.Trim() }
                };

                var response = await ApiClient.SignIn(new FormUrlEncodedContent(parameters));
                if (response.Contains("用户名和密码无法匹配"))
                {
                    Toast.ShowTips("账号密码或验证码不匹配");
                    TbCaptcha.Text = string.Empty;
                    await RefreshCaptchaAsync(showErrorToast: false);
                }
                else if (!response.Contains("登出"))
                {
                    Toast.ShowTips("登录失败，请检查验证码或网络");
                    TbCaptcha.Text = string.Empty;
                    await RefreshCaptchaAsync(showErrorToast: false);
                }
                else
                {
                    var localSettings = ApplicationData.Current.LocalSettings;
                    var cookies = ApiClient.Handler.CookieContainer.GetCookieHeader(new Uri("https://www.v2ex.com"));
                    localSettings.Values["Cookies"] = cookies;
                    App.Window.PageFrame.Navigate(typeof(MainPage));
                }
            }
            catch
            {
                Toast.ShowTips("网络连接异常，请稍后重试");
                await RefreshCaptchaAsync(showErrorToast: false);
            }
            finally
            {
                _isSubmitting = false;
                LoginRing.IsActive = false;
                LoginRing.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
                BtnLoginText.Text = "登录";
                _controls.ForEach(c => c.IsEnabled = true);
            }
        }
    }
}
