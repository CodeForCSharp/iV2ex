using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;
using iV2EX.Annotations;
using iV2EX.GetData;
using iV2EX.Model;
using iV2EX.Util;
using iV2EX.Views;
using PagingEx;
using System.Threading.Tasks;
using AngleSharp.Html.Parser;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

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
                        Username = tables[0].QuerySelector("span.bigger").TextContent
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
            var checkIn = Observable.FromEventPattern<TappedRoutedEventArgs>(CheckInItem, nameof(CheckInItem.Tapped))
                .Select(async x =>
                {
                    var html = await ApiClient.GetCheckInInformation();
                    var href = new HtmlParser().ParseDocument(html).GetElementById("Main").QuerySelector("input")
                        .GetAttribute("onclick").Replace("location.href = '", "").Replace("';", "").Trim();
                    if (href.Contains("/balance")) return CheckInStatus.Gone;
                    var r = await ApiClient.CheckIn($"https://www.v2ex.com{href}", $"https://www.v2ex.com{href}");
                    return CheckInStatus.Success;
                })
                .ObserveOnDispatcher()
                .Subscribe(async x =>
                {
                    switch (await x)
                    {
                        case CheckInStatus.Gone:
                            Toast.ShowTips("已经签到");
                            break;
                        case CheckInStatus.Success:
                            Toast.ShowTips("签到成功");
                            Observable.FromAsync(y => loadData())
                            .Retry(10)
                            .ObserveOnDispatcher()
                            .Subscribe(y => People = y);
                            break;
                        case CheckInStatus.Failure:
                            Toast.ShowTips("签到失败");
                            break;
                    }
                }, ex => Toast.ShowTips("签到失败"));
            var cancel = Observable.FromEventPattern<TappedRoutedEventArgs>(CancelItem, nameof(CancelItem.Tapped))
                .ObserveOnDispatcher()
                .Subscribe(x =>
                {
                    if (Window.Current.Content is ActivityContainer frame)
                    {
                        PageStack.Clear();
                        frame.ClearBackStack();
                        frame.Navigate(typeof(UserLoginView));
                    }
                });
            var write = Observable.FromEventPattern<TappedRoutedEventArgs>(WriteItem, nameof(WriteItem.Tapped))
                .Subscribe(x => PageStack.Next("Left", "Right", typeof(WriteTopicView), null));
            var user = Observable.FromEventPattern<TappedRoutedEventArgs>(UserItem, nameof(UserItem.Tapped))
                .Subscribe(x => PageStack.Next("Left", "Right", typeof(MemberView), People.Member.Username));
            var money = Observable.FromEventPattern<TappedRoutedEventArgs>(MoneyItem, nameof(MoneyItem.Tapped))
                .Subscribe(x => PageStack.Next("Left", "Right", typeof(MoneyDetailView), null));
            var collectTopic = Observable
                .FromEventPattern<TappedRoutedEventArgs>(CollectTopicItem, nameof(CollectTopicItem.Tapped))
                .Subscribe(x => PageStack.Next("Left", "Right", typeof(PeopleTopicView), Convert.ToInt32(People.CollectedTopics)));
            var collectNode = Observable.FromEventPattern<TappedRoutedEventArgs>(CollectNodeItem, nameof(CollectNodeItem.Tapped))
                .Subscribe(x => PageStack.Next("Left", "Right", typeof(PeopleNodeView), null));
            var message = Observable.FromEventPattern<TappedRoutedEventArgs>(MessageItem, nameof(MessageItem.Tapped))
                .Subscribe(x => PageStack.Next("Left", "Right", typeof(PeopleNotificationView), null));
            var follower = Observable.FromEventPattern<TappedRoutedEventArgs>(FollowerItem, nameof(FollowerItem.Tapped))
                .Subscribe(x => PageStack.Next("Left", "Right", typeof(PeopleFollowerView), null));
            var loadInformation = Observable
                .FromEventPattern<RoutedEventArgs>(UserInformationFragment, nameof(UserInformationFragment.Loaded))
                .SelectMany(x => loadData())
                .Retry(10)
                .ObserveOnDispatcher()
                .Subscribe(x => People = x);
            var refresh = Observable.FromEventPattern<TappedRoutedEventArgs>(Refresh, nameof(Refresh.Tapped))
                .SelectMany(x => loadData())
                .Retry(10)
                .ObserveOnDispatcher()
                .Subscribe(x => People = x);

            this.Unloaded += (s, e) =>
            {
                checkIn.Dispose();
                cancel.Dispose();
                write.Dispose();
                user.Dispose();
                money.Dispose();
                collectTopic.Dispose();
                collectNode.Dispose();
                message.Dispose();
                follower.Dispose();
                loadInformation.Dispose();
                refresh.Dispose();
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