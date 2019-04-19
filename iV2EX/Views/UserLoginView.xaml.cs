using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Reactive.Linq;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using iV2EX.GetData;
using iV2EX.Model;
using iV2EX.Util;
using PagingEx;
using System.Threading.Tasks;
using AngleSharp.Html.Parser;

namespace iV2EX.Views
{
    public sealed partial class UserLoginView
    {
        private LoginModel _data;

        private List<IDisposable> _events;
        public UserLoginView()
        {
            InitializeComponent();
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
            var controls = new List<Control> { TbPassword, TbUsername, BtnLogin, TbCaptcha };
            var login = Observable.FromEventPattern<TappedRoutedEventArgs>(BtnLogin, nameof(BtnLogin.Tapped))
                .Select(x =>
                {
                    controls.ForEach(y => y.IsEnabled = false);
                    return x;
                })
                .Select(async x =>
                {
                    if (string.IsNullOrEmpty(TbUsername.Text)) return SignInStatus.UsernameEmpty;
                    if (string.IsNullOrEmpty(TbPassword.Password)) return SignInStatus.PasswordEmpty;
                    if (string.IsNullOrEmpty(TbCaptcha.Text)) return SignInStatus.CaptchaEmpty;
                    var parmas = new Dictionary<string, string>
                    {
                        {"next", "/"},
                        {_data.UName, TbUsername.Text},
                        {"once", _data.Once},
                        {_data.PName, TbPassword.Password},
                        {_data.CName, TbCaptcha.Text}
                    };
                    var r = await ApiClient.SignIn(new FormUrlEncodedContent(parmas));
                    if (r.Contains("用户名和密码无法匹配")) return SignInStatus.UsernameOrPasswordError;
                    if (r.Contains("登出")) return SignInStatus.Success;
                    return SignInStatus.NetworkError;
                })
                .ObserveOnDispatcher()
                .Subscribe(async x =>
                {
                    try
                    {
                        switch (await x)
                        {
                            case SignInStatus.UsernameOrPasswordError:
                                Toast.ShowTips("账号密码不匹配");
                                throw new Exception();
                            case SignInStatus.NetworkError:
                                Toast.ShowTips("网络连接异常");
                                throw new Exception();
                            case SignInStatus.UsernameEmpty:
                                Toast.ShowTips("账号不能为空");
                                break;
                            case SignInStatus.PasswordEmpty:
                                Toast.ShowTips("密码不能为空");
                                break;
                            case SignInStatus.CaptchaEmpty:
                                Toast.ShowTips("验证码不能为空");
                                break;
                            case SignInStatus.Success:
                                var localSettings = ApplicationData.Current.LocalSettings;
                                var cookies =
                                    ApiClient.Handler.CookieContainer.GetCookieHeader(new Uri("https://www.v2ex.com"));
                                if (localSettings.Values["Cookies"] == null)
                                    localSettings.Values.Add("Cookies", cookies);
                                else
                                    localSettings.Values["Cookies"] = cookies;
                                if (Window.Current.Content is ActivityContainer mtFrame)
                                    mtFrame.Navigate(typeof(MainPage));
                                break;
                        }
                    }
                    catch
                    {
                        Observable.FromAsync(y => loginData())
                        .ObserveOnDispatcher()
                            .Subscribe(async y =>
                            {
                                _data = y;
                                CaptchaImage.Source = await GetBitmapFromUrl.GetBitmapFromStream(_data.CImage);
                            });
                    }
                    finally
                    {
                        controls.ForEach(y => y.IsEnabled = true);
                    }
                });
            var loadInformation = Observable
                .FromEventPattern<RoutedEventArgs>(UserLoginPage, nameof(UserLoginPage.Loaded))
                .SelectMany(x => loginData())
                .Retry(100)
                .ObserveOnDispatcher()
                .Subscribe(async x =>
                {
                    _data = x;
                    CaptchaImage.Source = await GetBitmapFromUrl.GetBitmapFromStream(_data.CImage);
                });

            _events = new List<IDisposable> { loadInformation, login };
        }

        protected internal override void OnDestroy()
        {
            base.OnDestroy();
            _events.ForEach(x => x.Dispose());
        }
    }
}