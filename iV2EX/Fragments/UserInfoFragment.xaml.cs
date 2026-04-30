using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using iV2EX.Annotations;
using iV2EX.GetData;
using iV2EX.Model;
using iV2EX.Util;
using iV2EX.Views;
using System.Threading.Tasks;
using AngleSharp.Html.Parser;
using Microsoft.UI.Xaml.Controls;

namespace iV2EX.Fragments
{
    public partial class UserInfoFragment : INotifyPropertyChanged
    {
        private PersonCenterModel _people = new PersonCenterModel();

        public UserInfoFragment()
        {
            InitializeComponent();
            async Task<PersonCenterModel> loadData()
            {
                var html = await ApiClient.GetMainPage();
                var right = new HtmlParser().ParseDocument(html).GetElementById("Rightbar");
                var tables = right.QuerySelectorAll("table");
                var spans = tables[1].QuerySelectorAll("span.bigger");
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
                    Money = right.QuerySelector("div#money").QuerySelector("a").InnerHtml,
                    Notifications = right.QuerySelector("a[href='/notifications']").TextContent.Split(' ')
                        .FirstOrDefault(),
                    IsNotChecked = right.QuerySelector("a[href='/mission/daily']") != null
                };
            }

            async Task RefreshData() =>
                People = await AsyncHelper.RetryAsync(() => loadData(), 5);

            CheckInItem.Tapped += async (s, e) =>
            {
                try
                {
                    var html = await ApiClient.GetCheckInInformation();
                    var href = new HtmlParser().ParseDocument(html).GetElementById("Main").QuerySelector("input")
                        .GetAttribute("onclick").Replace("location.href = '", "").Replace("';", "").Trim();
                    if (href.Contains("/balance"))
                    {
                        Toast.ShowTips("已经签到");
                        return;
                    }
                    await ApiClient.CheckIn($"https://www.v2ex.com{href}", $"https://www.v2ex.com{href}");
                    Toast.ShowTips("签到成功");
                    await RefreshData();
                }
                catch
                {
                    Toast.ShowTips("签到失败");
                }
            };

            CancelItem.Tapped += (s, e) =>
            {
                PageStack.Clear();
                App.Window.PageFrame.BackStack.Clear();
                App.Window.PageFrame.Navigate(typeof(UserLoginView));
            };

            WriteItem.Tapped += (s, e) =>
            {
                PageStack.Next("Left", "Right", typeof(WriteTopicView), null);
            };

            UserItem.Tapped += (s, e) =>
            {
                PageStack.Next("Left", "Right", typeof(MemberView), People.Member.Username);
            };

            MoneyItem.Tapped += (s, e) =>
            {
                PageStack.Next("Left", "Right", typeof(MoneyDetailView), null);
            };

            CollectTopicItem.Tapped += (s, e) =>
            {
                PageStack.Next("Left", "Right", typeof(PeopleTopicView), Convert.ToInt32(People.CollectedTopics));
            };

            CollectNodeItem.Tapped += (s, e) =>
            {
                PageStack.Next("Left", "Right", typeof(PeopleNodeView), null);
            };

            MessageItem.Tapped += (s, e) =>
            {
                PageStack.Next("Left", "Right", typeof(PeopleNotificationView), null);
            };

            FollowerItem.Tapped += (s, e) =>
            {
                PageStack.Next("Left", "Right", typeof(PeopleFollowerView), null);
            };

            UserInformationFragment.Loaded += async (s, e) =>
            {
                People = await AsyncHelper.RetryAsync(() => loadData(), 5);
            };

            Refresh.Tapped += async (s, e) =>
            {
                People = await AsyncHelper.RetryAsync(() => loadData(), 5);
            };
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

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
