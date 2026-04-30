using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using iV2EX.Annotations;
using iV2EX.GetData;
using iV2EX.Model;
using iV2EX.TupleModel;
using iV2EX.Util;
using AngleSharp.Html.Parser;
using System.Collections.Generic;
using Microsoft.UI.Xaml.Navigation;

namespace iV2EX.Views
{
    public partial class OneNodeTopicsView : INotifyPropertyChanged
    {
        private NodeModel _node = new NodeModel();

        public OneNodeTopicsView()
        {
            InitializeComponent();

            NodeTopcisList.ItemClick += (s, e) =>
            {
                if (e.ClickedItem is TopicModel item)
                    PageStack.Next("Right", "Right", typeof(RepliesAndTopicView), item.Id);
            };

            CollectNode.Tapped += async (s, e) =>
            {
                var html = await ApiClient.GetNodeInformation(Node.Name);
                var regexFav = new Regex("<a href=\"(.*)\">加入收藏</a>");
                var regexUnFav = new Regex("<a href=\"(.*)\">取消收藏</a>");
                var url = "";
                if (regexFav.IsMatch(html)) url = regexFav.Match(html).Groups[1].Value;
                if (regexUnFav.IsMatch(html)) url = regexUnFav.Match(html).Groups[1].Value;
                await ApiClient.OnlyGet($"https://www.v2ex.com{url}");
                Node.IsCollect = Node.IsCollect == "加入收藏" ? "取消收藏" : "加入收藏";
            };

            NotifyData.LoadDataTask = async count =>
            {
                var html = await ApiClient.GetTopicsWithPageN(Node.Name, NotifyData.CurrentPage);
                var dom = new HtmlParser().ParseDocument(html);
                if (NotifyData.MaxPage == 0)
                {
                    var header = dom.GetElementById("Main").QuerySelector("div.node_header");
                    Node = new NodeModel
                    {
                        Topics = Convert.ToInt32(header.QuerySelector("strong").TextContent),
                        IsCollect = header.QuerySelector("a").TextContent,
                        Cover = header.QuerySelector("img") == null
                            ? "ms-appx:///Assets/default.png"
                            : header.QuerySelector("img").GetAttribute("src"),
                        Title = Node.Title,
                        Name = Node.Name
                    };
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
                            Image = imgs.GetAttribute("src")
                        },
                        Id = int.Parse(node.ClassName.Split('_').Last())
                    };
                    if (hrefs.Length == 5)
                    {
                        topic.LastUsername = $"最后回复者 :{hrefs[3].TextContent}";
                        topic.Replies = int.Parse(hrefs[4].TextContent);
                        var last = node.GetElementsByClassName("topic_info").First().TextContent.Split('•')[1].Trim();
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

        public IncrementalLoadingCollection<TopicModel> NotifyData { get; } = new IncrementalLoadingCollection<TopicModel>();

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

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            var parameter = e.Parameter;
            if (parameter is NodeModel node)
            {
                Node.Title = node.Title;
                Node.Name = node.Name;
            }
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
