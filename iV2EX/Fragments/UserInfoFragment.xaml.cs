using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using iV2EX.Annotations;
using iV2EX.GetData;
using iV2EX.Model;
using iV2EX.Util;
using iV2EX.Views;

namespace iV2EX.Fragments
{
    public partial class UserInfoFragment : INotifyPropertyChanged
    {
        private static readonly TimeSpan AutoRefreshInterval = TimeSpan.FromSeconds(10);
        private PersonCenterModel _people = new();
        private bool _isRefreshing;
        private DateTimeOffset _lastRefreshedAt = DateTimeOffset.MinValue;

        public UserInfoFragment()
        {
            InitializeComponent();
        }

        public PersonCenterModel People
        {
            get => _people;
            set
            {
                _people = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private async void UserInfoFragment_Loaded(object sender, RoutedEventArgs e)
        {
            await RefreshIfStaleAsync();
        }

        public void RefreshIfStale()
        {
            _ = RefreshIfStaleAsync();
        }

        private async Task RefreshIfStaleAsync()
        {
            if (_isRefreshing) return;
            if (DateTimeOffset.UtcNow - _lastRefreshedAt < AutoRefreshInterval) return;
            await RefreshAsync(showErrorToast: false);
        }

        private async void Refresh_Click(object sender, RoutedEventArgs e)
        {
            await RefreshAsync(showErrorToast: true);
        }

        private async Task RefreshAsync(bool showErrorToast)
        {
            if (_isRefreshing) return;
            _isRefreshing = true;
            RefreshBtn.IsEnabled = false;
            LoadingBar.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
            try
            {
                var data = await AsyncHelper.RetryAsync(LoadData, 3);
                if (data != null)
                {
                    People = data;
                    _lastRefreshedAt = DateTimeOffset.UtcNow;
                }
            }
            catch
            {
                if (showErrorToast)
                    Toast.ShowTips("刷新失败，请稍后重试");
            }
            finally
            {
                _isRefreshing = false;
                RefreshBtn.IsEnabled = true;
                LoadingBar.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
            }
        }

        private async void CheckInItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var html = await ApiClient.GetCheckInInformation();
                var href = new HtmlParser().ParseDocument(html).GetElementById("Main")
                    .QuerySelector("input")
                    .GetAttribute("onclick").Replace("location.href = '", "").Replace("';", "").Trim();
                if (href.Contains("/balance"))
                {
                    Toast.ShowTips("已经签到");
                    return;
                }

                await ApiClient.CheckIn($"https://www.v2ex.com{href}", $"https://www.v2ex.com{href}");
                Toast.ShowTips("签到成功");
                await RefreshAsync(showErrorToast: false);
            }
            catch
            {
                Toast.ShowTips("签到失败");
            }
        }

        private void UserItem_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(People?.Member?.Username))
                PageStack.Next("Left", "Right", typeof(MemberView), People.Member.Username);
        }

        private void MoneyItem_Click(object sender, RoutedEventArgs e)
        {
            PageStack.Next("Left", "Right", typeof(MoneyDetailView), null);
        }

        private void CollectTopicItem_Click(object sender, RoutedEventArgs e)
        {
            PageStack.Next("Left", "Right", typeof(PeopleTopicView), Convert.ToInt32(People.CollectedTopics));
        }

        private void CollectNodeItem_Click(object sender, RoutedEventArgs e)
        {
            PageStack.Next("Left", "Right", typeof(PeopleNodeView), null);
        }

        private void FollowerItem_Click(object sender, RoutedEventArgs e)
        {
            PageStack.Next("Left", "Right", typeof(PeopleFollowerView), null);
        }

        private void MessageItem_Click(object sender, RoutedEventArgs e)
        {
            PageStack.Next("Left", "Right", typeof(PeopleNotificationView), null);
        }

        private void HistoryItem_Click(object sender, RoutedEventArgs e)
        {
            PageStack.Next("Left", "Right", typeof(BrowseHistoryView), null);
        }

        private void WriteItem_Click(object sender, RoutedEventArgs e)
        {
            PageStack.Next("Left", "Right", typeof(WriteTopicView), null);
        }

        private async void CancelItem_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new ContentDialog
            {
                Title = "退出账号",
                Content = "确定要退出当前账号吗？",
                PrimaryButtonText = "退出",
                CloseButtonText = "取消",
                DefaultButton = ContentDialogButton.Close,
                XamlRoot = XamlRoot,
            };

            if (await dialog.ShowAsync() == ContentDialogResult.Primary)
            {
                PageStack.Clear();
                App.Window.PageFrame.BackStack.Clear();
                App.Window.PageFrame.Navigate(typeof(UserLoginView));
            }
        }

        private async Task<PersonCenterModel> LoadData()
        {
            var html = await ApiClient.GetMainPage();
            var right = new HtmlParser().ParseDocument(html).GetElementById("Rightbar");
            var tables = right.QuerySelectorAll("table");
            var spans = tables[1].QuerySelectorAll("span.bigger");
            var moneyLink = right.QuerySelector("div#money a");
            return new PersonCenterModel
            {
                Member = new MemberModel
                {
                    Image = tables[0].QuerySelector("img").GetAttribute("src"),
                    Username = tables[0].QuerySelector("span.bigger").QuerySelector("a").TextContent,
                },
                CollectedNodes = spans[0].TextContent,
                CollectedTopics = spans[1].TextContent,
                NoticePeople = spans[2].TextContent,
                Gold = ParseCoin(moneyLink, "G"),
                Silver = ParseCoin(moneyLink, "S"),
                Bronze = ParseCoin(moneyLink, "B"),
                Notifications = right.QuerySelector("a[href='/notifications']").TextContent.Split(' ')
                    .FirstOrDefault(),
                IsNotChecked = right.QuerySelector("a[href='/mission/daily']") != null,
                ActivityPercent = ParseActivityPercent(right)
            };
        }

        private static double ParseActivityPercent(IElement right)
        {
            var style = right?.QuerySelector(".member-activity-bar > *")?.GetAttribute("style") ?? "";
            var match = Regex.Match(style, @"width\s*:\s*([\d.]+)");
            return match.Success && double.TryParse(match.Groups[1].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var percent)
                ? Math.Clamp(percent, 0, 100)
                : 0;
        }

        private static int ParseCoin(IElement moneyLink, string alt)
        {
            var img = moneyLink?.QuerySelector($"img[alt='{alt}']");
            if (img?.PreviousSibling == null) return 0;
            return int.TryParse(img.PreviousSibling.TextContent?.Trim(), out var amount) ? amount : 0;
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
