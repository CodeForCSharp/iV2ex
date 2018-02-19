using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using AngleSharp.Parser.Html;
using iV2EX.Annotations;
using iV2EX.GetData;
using iV2EX.Model;
using iV2EX.TupleModel;
using iV2EX.Util;
using MyToolkit.Paging;

namespace iV2EX.Views
{
    public partial class OneNodeTopicsView : INotifyPropertyChanged
    {
        private NodeModel _node = new NodeModel();

        public OneNodeTopicsView()
        {
            InitializeComponent();
            var client = ApiClient.Client;
            var click = Observable
                .FromEventPattern<ItemClickEventArgs>(NodeTopcisList, nameof(NodeTopcisList.ItemClick))
                .ObserveOnDispatcher()
                .Subscribe(x =>
                {
                    var item = x.EventArgs.ClickedItem as TopicModel;
                    PageStack.Next("Right", "Right", typeof(RepliesAndTopicView), item.Id);
                });
            var collect = Observable.FromEventPattern<TappedRoutedEventArgs>(CollectNode, nameof(CollectNode.Tapped))
                .ObserveOnDispatcher()
                .Subscribe(async x =>
                {
                    try
                    {
                        var html = await client.GetNodeInformation(Node.Name);
                        var regexFav = new Regex("<a href=\"(.*)\">加入收藏</a>");
                        var regexUnFav = new Regex("<a href=\"(.*)\">取消收藏</a>");
                        var url = "";
                        if (regexFav.IsMatch(html)) url = regexFav.Match(html).Groups[1].Value;
                        if (regexUnFav.IsMatch(html)) url = regexUnFav.Match(html).Groups[1].Value;
                        await client.OnlyGet($"https://www.v2ex.com{url}");
                        Node.IsCollect = Node.IsCollect == "加入收藏" ? "取消收藏" : "加入收藏";
                    }
                    catch
                    {
                    }
                });
            NotifyData.LoadDataTask = async count =>
            {
                var html = await client.GetTopicsWithPageN(Node.Name, NotifyData.CurrentPage);
                var dom = new HtmlParser().Parse(html);
                if (NotifyData.MaxPage == 0)
                {
                    var header = dom.GetElementById("Main").QuerySelector("div.node_header");
                    Node.Topics = Convert.ToInt32(header.QuerySelector("strong").TextContent);
                    Node.IsCollect = header.QuerySelector("a").TextContent;
                    Node.Cover = header.QuerySelector("img") == null
                        ? "ms-appx:///Assets/default.png"
                        : $"https:{header.QuerySelector("img").GetAttribute("src")}";
                }

                var topics = dom.GetElementById("TopicsNode").Children.Select(node =>
                {
                    var hrefs = node.QuerySelectorAll("a");
                    var imgs = node.QuerySelector("img.avatar");
                    var topic = new TopicModel
                    {
                        Title = hrefs[1].TextContent,
                        Member = new MemberModel
                        {
                            Username = hrefs[2].TextContent,
                            Image = $"https:{imgs.GetAttribute("src")}"
                        },
                        Id = int.Parse(node.ClassName.Split('_').Last())
                    };
                    if (hrefs.Length == 5)
                    {
                        topic.LastUsername = $"最后回复者 :{hrefs[3].TextContent}";
                        topic.Replies = int.Parse(hrefs[4].TextContent);
                        var last = node.GetElementsByClassName("small fade").First().TextContent.Split('•')[1].Trim();
                        if (last.Length > 12)
                        {
                            var timeSpan = DateTime.Now - DateTime.Parse(last.Insert(10, " "));
                            last = $"{(int) timeSpan.TotalDays}天";
                        }

                        topic.LastReply = $"时间 : {last.Trim()}";
                    }

                    return topic;
                });
                return new PagesBaseModel<TopicModel>
                {
                    Pages = Node.Topics % 20 == 0 ? Node.Topics / 20 : Node.Topics / 20 + 1,
                    Entity = topics
                };
            };
        }

        public IncrementalLoadingCollection<TopicModel> NotifyData { get; } =
            new IncrementalLoadingCollection<TopicModel>();

        public NodeModel Node
        {
            get => _node;
            set
            {
                _node = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected internal override void OnNavigatedTo(MtNavigationEventArgs e)
        {
            if (e.Parameter is NodeModel node)
            {
                Node.Title = node.Title;
                Node.Name = node.Name;
            }

            base.OnNavigatedTo(e);
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}