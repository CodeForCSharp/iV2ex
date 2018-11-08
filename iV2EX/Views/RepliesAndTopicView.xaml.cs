using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Windows.ApplicationModel.DataTransfer;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using AngleSharp.Parser.Html;
using iV2EX.Annotations;
using iV2EX.GetData;
using iV2EX.Model;
using iV2EX.TupleModel;
using iV2EX.Util;
using Microsoft.Toolkit.Uwp.UI.Controls;

namespace iV2EX.Views
{
    public partial class RepliesAndTopicView : INotifyPropertyChanged
    {
        private int _id;

        private int _replyfloor;

        private TopicModel _topic = new TopicModel();

        public RepliesAndTopicView()
        {
            InitializeComponent();
            var controls = new List<Control> {Send, ReplyText};
            var send = Observable.FromEventPattern<RoutedEventArgs>(Send, nameof(Send.Click))
                .Select(x =>
                {
                    controls.ForEach(c => c.IsEnabled = false);
                    return x;
                })
                .Select(async x =>
                {
                    var content = ReplyText.Text;
                    if (string.IsNullOrEmpty(content)) return ReplyStatus.TextEmpty;
                    var html = await ApiClient.GetTopicInformation(_id);
                    var once = new HtmlParser().Parse(html).QuerySelector("input[name='once']").GetAttribute("value");
                    var pramas = new Dictionary<string, string>
                    {
                        {"content", content},
                        {"once", once}
                    };
                    var text = await ApiClient.ReplyTopic($"https://www.v2ex.com/t{_id}",
                        new FormUrlEncodedContent(pramas), _id);
                    if (text.Contains("你回复过于频繁了")) return ReplyStatus.Ban;
                    return ReplyStatus.Success;
                })
                .ObserveOnDispatcher()
                .Subscribe(async x =>
                {
                    try
                    {
                        switch (await x)
                        {
                            case ReplyStatus.Ban:
                                Toast.ShowTips("您被禁言1800秒");
                                break;
                            case ReplyStatus.TextEmpty:
                                Toast.ShowTips("内容不能为空");
                                break;
                            case ReplyStatus.Success:
                                Toast.ShowTips("回复成功");
                                ReplyText.Text = "";
                                break;
                        }
                    }
                    catch
                    {
                        Toast.ShowTips("回复失败");
                    }
                    finally
                    {
                        controls.ForEach(c => c.IsEnabled = true);
                    }
                });
            var at = Observable.FromEventPattern<TappedRoutedEventArgs>(UsernamePanel, nameof(UsernamePanel.Tapped))
                .ObserveOnDispatcher()
                .Subscribe(x => ReplyText.Text += $"@{(string) (x.Sender as TextBlock).Tag} ");
            var collect = Observable
                .FromEventPattern<TappedRoutedEventArgs>(CollectedPanel, nameof(CollectedPanel.Tapped))
                .ObserveOnDispatcher()
                .Subscribe(async x =>
                {
                    try
                    {
                        var html = await ApiClient.GetTopicInformation(_id);
                        var url = "";
                        var regexFav = new Regex("<a href=\"(.*)\">加入收藏</a>");
                        var regexUnFav = new Regex("<a href=\"(.*)\">取消收藏</a>");
                        if (regexFav.IsMatch(html)) url = regexFav.Match(html).Groups[1].Value;
                        if (regexUnFav.IsMatch(html)) url = regexUnFav.Match(html).Groups[1].Value;
                        await ApiClient.OnlyGet($"https://www.v2ex.com{url}");
                        if (Topic.Collect == "加入\n收藏")
                        {
                            Topic.Collect = "已\n收藏";
                            Toast.ShowTips("收藏成功");
                        }
                        else
                        {
                            Topic.Collect = "加入\n收藏";
                            Toast.ShowTips("取消收藏成功");
                        }
                    }
                    catch
                    {
                        Toast.ShowTips("操作失败");
                    }
                });
            var tImageTap = Observable.FromEventPattern<TappedRoutedEventArgs>(TImagePanel, nameof(TImagePanel.Tapped))
                .ObserveOnDispatcher()
                .Subscribe(x => PageStack.Next("Right", "Right", typeof(MemberView), TImagePanel.Tag));
            var copyLink = Observable
                .FromEventPattern<TappedRoutedEventArgs>(CopyLinkPanel, nameof(CopyLinkPanel.Tapped))
                .ObserveOnDispatcher()
                .Subscribe(x =>
                {
                    var clipBoardText = new DataPackage();
                    clipBoardText.SetText(Topic.Url);
                    Clipboard.SetContent(clipBoardText);
                    Toast.ShowTips("已复制到剪贴板");
                });
            Replies.LoadDataTask = async count =>
            {
                var html = await ApiClient.GetRepliesAndTopicContent(_id, 1);
                var main = new HtmlParser().Parse(html).GetElementById("Main");
                if (Replies.MaxPage == 0)
                    try
                    {
                        var numberString = Regex.IsMatch(html, "([0-9]{1,}) 回复")
                            ? Regex.Match(html, "([0-9]{1,}) 回复").Groups[1].Value
                            : "0";
                        var maxReply = int.Parse(numberString);
                        var node = main.QuerySelector("div.header");
                        var topic = new TopicModel
                        {
                            Id = _id,
                            Member = new MemberModel
                            {
                                Image = $"http:{node.QuerySelector("img").GetAttribute("src")}",
                                Username = node.QuerySelector("a").GetAttribute("href").Split('/')[2]
                            },
                            Title = node.QuerySelector("h1").TextContent,
                            Url = $"https://www.v2ex.com/t/{_id}",
                            Postscript = main.QuerySelectorAll("div.subtle").Select(subtle => new TopicModel
                            {
                                Content = subtle.QuerySelector("div.topic_content").InnerHtml,
                                LastReply = subtle.QuerySelector("span.fade").TextContent
                            }).ToList(),
                            Collect = main.TextContent.Contains("加入收藏") ? "加入\n收藏" : "已\n收藏",
                            Content = main.QuerySelector("div.topic_content")?.InnerHtml,
                            Replies = maxReply,
                            CreateDate = node.QuerySelector("small.gray").TextContent.Split('·')[1]
                        };
                        Topic = topic;
                    }
                    catch
                    {
                        Toast.ShowTips("加载失败，没有权限");
                        return new PagesBaseModel<ReplyModel>
                        {
                            Pages = 0,
                            Entity = new List<ReplyModel>()
                        };
                    }

                var replies = main.QuerySelectorAll("table").Where(table => table.ParentElement.Id != null)
                    .Select(table =>
                    {
                        var spans = table.QuerySelectorAll("span");
                        return new ReplyModel
                        {
                            Id = int.Parse(table.ParentElement.Id.Replace("r_", "")),
                            Avater = $"http:{table.QuerySelector("img").GetAttribute("src")}",
                            Username = table.QuerySelector("strong").TextContent,
                            Content = table.QuerySelector("div.reply_content").InnerHtml.Trim(),
                            Thanks = spans.Length == 3 ? int.Parse(spans[2].TextContent.Replace("♥ ", "")) : 0,
                            Floor = $"#{spans[0].TextContent}",
                            ReplyDate = spans[1].TextContent,
                            IsLz = Topic.Member.Username == table.QuerySelector("strong").TextContent
                        };
                    });
                return new PagesBaseModel<ReplyModel>
                {
                    Pages = Topic.Replies % 100 == 0 ? Topic.Replies / 100 : Topic.Replies / 100 + 1,
                    Entity = replies
                };
            };
        }

        private IncrementalLoadingCollection<ReplyModel> Replies { get; } =
            new IncrementalLoadingCollection<ReplyModel>();

        private TopicModel Topic
        {
            get => _topic;
            set
            {
                _topic = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected internal override void OnCreate(object parameter)
        {
            if (parameter is int i)
            {
                _id = i;
            }
            else if (parameter is Tuple<int, int> tuple)
            {
                _id = tuple.Item1;
                _replyfloor = tuple.Item2;
            }
            base.OnCreate(parameter);
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void ImagePanel_Tapped(object sender, TappedRoutedEventArgs e)
        {
            PageStack.Next("Right", "Right", typeof(MemberView), (string) (sender as ImageEx).Tag);
        }

        private void UsernamePanel_Tapped(object sender, TappedRoutedEventArgs e)
        {
            ReplyText.Text += $"@{(string) (sender as TextBlock).Tag} ";
        }
    }
}