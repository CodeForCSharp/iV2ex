using System;
using System.Collections.Generic;
using System.Net.Http;
using Windows.Storage;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using iV2EX.GetData;
using iV2EX.Model;
using iV2EX.Util;
using System.Threading.Tasks;
using AngleSharp.Html.Parser;

namespace iV2EX.Views
{
    public sealed partial class UserLoginView
    {
        private LoginModel _data;

        private List<Control> _controls;

        public UserLoginView()
        {
            InitializeComponent();

            async Task RefreshCaptcha()
            {
                _data = await loginData();
                CaptchaImage.Source = await GetBitmapFromUrl.GetBitmapFromStream(_data.CImage);
            }

            async Task<LoginModel> loginData()
            {
                var html = await ApiClient.GetSignInInformation();
                var form = new HtmlParser().ParseDocument(html).QuerySelector("form[action='/signin']");
                var inputs = form.QuerySelectorAll("input");
                return new LoginModel
                {
                    UName = inputs[0].GetAttribute("name"),
                    PName = inputs[1].GetAttribute("name"),
                    CName = inputs[2].GetAttribute("name"),
                    Once = inputs[3].GetAttribute("value"),
                    CImage = $"https://www.v2ex.com/_captcha?once={inputs[3]?.GetAttribute("value")}"
                };
            }

            _controls = new List<Control> { TbPassword, TbUsername, BtnLogin, TbCaptcha };

            BtnLogin.Tapped += async (s, e) =>
            {
                if (string.IsNullOrEmpty(TbUsername.Text)) { Toast.ShowTips("账号不能为空"); return; }
                if (string.IsNullOrEmpty(TbPassword.Password)) { Toast.ShowTips("密码不能为空"); return; }
                if (string.IsNullOrEmpty(TbCaptcha.Text)) { Toast.ShowTips("验证码不能为空"); return; }

                _controls.ForEach(y => y.IsEnabled = false);
                try
                {
                    var parmas = new Dictionary<string, string>
                    {
                        {"next", "/"},
                        {_data.UName, TbUsername.Text},
                        {"once", _data.Once},
                        {_data.PName, TbPassword.Password},
                        {_data.CName, TbCaptcha.Text}
                    };
                    var r = await ApiClient.SignIn(new FormUrlEncodedContent(parmas));
                    if (r.Contains("用户名和密码无法匹配"))
                    {
                        Toast.ShowTips("账号密码不匹配");
                        await RefreshCaptcha();
                    }
                    else if (!r.Contains("登出"))
                    {
                        Toast.ShowTips("网络连接异常");
                        await RefreshCaptcha();
                    }
                    else
                    {
                        var localSettings = ApplicationData.Current.LocalSettings;
                        var cookies =
                            ApiClient.Handler.CookieContainer.GetCookieHeader(new Uri("https://www.v2ex.com"));
                        if (localSettings.Values["Cookies"] == null)
                            localSettings.Values.Add("Cookies", cookies);
                        else
                            localSettings.Values["Cookies"] = cookies;
                        App.Window.PageFrame.Navigate(typeof(MainPage));
                    }
                }
                catch
                {
                    await RefreshCaptcha();
                }
                finally
                {
                    _controls.ForEach(y => y.IsEnabled = true);
                }
            };

            UserLoginPage.Loaded += async (s, e) =>
            {
                _data = await AsyncHelper.RetryAsync(() => loginData(), 5);
                CaptchaImage.Source = await GetBitmapFromUrl.GetBitmapFromStream(_data.CImage);
            };
        }
    }
}
