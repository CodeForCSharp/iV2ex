using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using iV2EX.Annotations;
using iV2EX.GetData;
using iV2EX.Model;
using iV2EX.TupleModel;
using iV2EX.Util;
using System.Threading.Tasks;
using AngleSharp.Html.Parser;
using Microsoft.UI.Xaml.Navigation;

namespace iV2EX.Views
{
    public partial class MemberView : INotifyPropertyChanged
    {
        private MemberModel _member = new MemberModel();
        private string _username;

        public MemberView()
        {
            InitializeComponent();
            async Task<MemberModel> loadData()
            {
                var html = await ApiClient.GetMemberInformation(_username);
                var cell = new HtmlParser().ParseDocument(html).GetElementById("Main").QuerySelector("div.cell");
                var inputs = cell.QuerySelectorAll("input");
                if (inputs.Length == 0)
                    return new MemberModel
                    {
                        Image = cell.QuerySelector("img").GetAttribute("src"),
                        Username = cell.QuerySelector("h1").TextContent
                    };
                return new MemberModel
                {
                    Image = cell.QuerySelector("img").GetAttribute("src"),
                    Notice = "https://www.v2ex.com" + inputs[0].GetAttribute("onclick").Split('\'')[3],
                    IsNotice = inputs[0].GetAttribute("value"),
                    Block = "https://www.v2ex.com" + inputs[1].GetAttribute("onclick").Split('\'')[3],
                    IsBlock = inputs[1].GetAttribute("value"),
                    Username = cell.QuerySelector("h1").TextContent
                };
            }

            MemberPage.Loaded += async (s, e) =>
            {
                Member = await AsyncHelper.RetryAsync(() => loadData(), 5);
            };

            Notice.Tapped += async (s, e) =>
            {
                await ApiClient.OnlyGet(Member.Notice);
                Member.IsNotice = "取消特别关注";
            };

            Block.Tapped += async (s, e) =>
            {
                await ApiClient.OnlyGet(Member.Block);
                Member.IsBlock = "取消Block";
            };

            MemberInfoList.ItemClick += (s, e) =>
            {
                if (e.ClickedItem is TopicModel item)
                    PageStack.Next("Right", "Right", typeof(RepliesAndTopicView), item.Id);
            };

            Topics.LoadDataTask = async count =>
            {
                var html = await ApiClient.GetTopicsByUsername(_username, Topics.CurrentPage);
                if (html.Contains("主题列表被隐藏"))
                {
                    ListHiddenPanel.Visibility = Visibility.Visible;
                    return new PagesBaseModel<TopicModel>
                    {
                        Pages = 0,
                        Entity = new List<TopicModel>()
                    };
                }

                var dom = new HtmlParser().ParseDocument(html);
                var pages = DomParse.ParseMaxPage(dom);
                var topics = dom.GetElementById("Main").GetElementsByClassName("cell item").Select(node =>
                {
                    var hrefs = node.QuerySelectorAll("a");
                    var topic = new TopicModel
                    {
                        Title = hrefs[0].TextContent,
                        NodeName = hrefs[1].TextContent,
                        Member = new MemberModel
                        {
                            Username = hrefs[2].TextContent
                        },
                        Id = int.Parse(new Regex("/t/([0-9]*)").Match(hrefs[0].GetAttribute("href")).Groups[1].Value)
                    };
                    if (hrefs.Length == 5)
                    {
                        topic.LastUsername = $"最后回复者 :{hrefs[3].TextContent}";
                        topic.Replies = int.Parse(hrefs[4].TextContent);
                        var last = node.QuerySelector("span.topic_info").TextContent.Split('•')[2].Trim();
                        last = last.Contains("最后回复") ? "" : last;
                        topic.LastReply = $"时间 : {last.Trim()}";
                    }

                    return topic;
                });
                return new PagesBaseModel<TopicModel>
                {
                    Pages = pages,
                    Entity = topics
                };
            };
            Notifications.LoadDataTask = async count =>
            {
                var html = await ApiClient.GetRepliesByUsername(_username, Notifications.CurrentPage);
                var dom = new HtmlParser().ParseDocument(html);
                var main = dom.GetElementById("Main");
                var nodes = main.QuerySelectorAll("div.dock_area");
                var replies = main.QuerySelectorAll("div.reply_content");
                var pages = DomParse.ParseMaxPage(dom);
                var notifications = nodes.Select(
                    (node, i) =>
                    {
                        var href = node.QuerySelector("a");
                        var span = node.QuerySelector("span");
                        return new NotificationModel
                        {
                            Title = href.TextContent,
                            Content = replies[i].TextContent,
                            ReplyDate = span.TextContent
                        };
                    });
                return new PagesBaseModel<NotificationModel>
                {
                    Pages = pages,
                    Entity = notifications
                };
            };
        }

        private MemberModel Member
        {
            get => _member;
            set
            {
                _member = value;
                OnPropertyChanged();
            }
        }

        public IncrementalLoadingCollection<TopicModel> Topics { get; } =
            new IncrementalLoadingCollection<TopicModel>();

        public IncrementalLoadingCollection<NotificationModel> Notifications { get; } =
            new IncrementalLoadingCollection<NotificationModel>();

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            var parameter = e.Parameter;
            if (parameter is string s) _username = s;
        }
    }
}
