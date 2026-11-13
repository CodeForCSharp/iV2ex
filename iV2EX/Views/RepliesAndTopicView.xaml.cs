using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Windows.ApplicationModel.DataTransfer;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using iV2EX.Annotations;
using iV2EX.GetData;
using iV2EX.Model;
using iV2EX.TupleModel;
using iV2EX.Util;
using AngleSharp.Html.Parser;
using Microsoft.UI.Xaml.Navigation;

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
            Replies.LoadingStateChanged += isLoading =>
            {
                if (isLoading)
                {
                    if (string.IsNullOrEmpty(Topic.Title))
                    {
                        LoadingRing.IsActive = true;
                        LoadingRing.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        LoadingBar.Visibility = Visibility.Visible;
                    }
                }
                else
                {
                    LoadingRing.IsActive = false;
                    LoadingRing.Visibility = Visibility.Collapsed;
                    LoadingBar.Visibility = Visibility.Collapsed;
                }
            };
            Replies.LoadFailed += _ => Toast.ShowTips("加载失败，请稍后重试");
            var controls = new List<Control> { Send, ReplyText };

            Send.Click += async (s, e) =>
            {
                var content = ReplyText.Text;
                if (string.IsNullOrEmpty(content))
                {
                    Toast.ShowTips("内容不能为空");
                    return;
                }

                controls.ForEach(c => c.IsEnabled = false);
                try
                {
                    var html = await ApiClient.GetTopicInformation(_id);
                    var once = new HtmlParser().ParseDocument(html).QuerySelector("input[name='once']").GetAttribute("value");
                    var pramas = new Dictionary<string, string>
                    {
                        {"content", content},
                        {"once", once}
                    };
                    var text = await ApiClient.ReplyTopic($"https://www.v2ex.com/t{_id}",
                        new FormUrlEncodedContent(pramas), _id);
                    if (text.Contains("你回复过于频繁了"))
                        Toast.ShowTips("您被禁言1800秒");
                    else
                    {
                        Toast.ShowTips("回复成功");
                        ReplyText.Text = "";
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
            };

            Replies.LoadDataTask = async count =>
            {
                var html = await ApiClient.GetRepliesAndTopicContent(_id, Replies.CurrentPage);
                var main = new HtmlParser().ParseDocument(html).GetElementById("Main");
                if (Replies.MaxPage == 0)
                {
                    try
                    {
                        var match = Regex.Match(html, "([0-9]{1,}) 条回复");
                        var replyString = match.Success ? match.Groups[1].Value : "0";
                        var maxReply = int.Parse(replyString);
                        var node = main.QuerySelector("div.header");
                        var nodeLink = node.QuerySelector("a[href^='/go/']");
                        var isCollected = !main.TextContent.Contains("加入收藏");
                        var topic = new TopicModel
                        {
                            Id = _id,
                            Member = new MemberModel
                            {
                                Image = node.QuerySelector("img").GetAttribute("src"),
                                Username = node.QuerySelector("a").GetAttribute("href").Split('/')[2]
                            },
                            Title = node.QuerySelector("h1").TextContent,
                            Url = $"https://www.v2ex.com/t/{_id}",
                            Postscript = main.QuerySelectorAll("div.subtle").Select(subtle => new TopicModel
                            {
                                Content = subtle.QuerySelector("div.topic_content").InnerHtml,
                                LastReply = subtle.QuerySelector("span.fade").TextContent
                            }).ToList(),
                            IsCollected = isCollected,
                            Collect = isCollected ? "已收藏" : "收藏",
                            Content = main.QuerySelector("div.topic_content")?.InnerHtml,
                            Replies = maxReply,
                            CreateDate = node.QuerySelector("small.gray span").TextContent.Trim(),
                            NodeName = nodeLink?.TextContent?.Trim(),
                            NodeSlug = nodeLink?.GetAttribute("href")?.Replace("/go/", "").Trim('/')
                        };
                        Topic = topic;
                        _ = BrowseHistoryStore.AddAsync(topic);
                    }
                    catch
                    {
                        Toast.ShowTips("加载失败，可能无权访问该主题");
                        return new PagesBaseModel<ReplyModel>
                        {
                            Pages = 0,
                            Entity = new List<ReplyModel>()
                        };
                    }
                }

                var lzUsername = Topic.Member?.Username;
                var replies = main.QuerySelectorAll("div.cell[id^='r_']").Select(cell =>
                {
                    var username = cell.QuerySelector("strong")?.TextContent?.Trim() ?? "";
                    var thanksMatch = Regex.Match(cell.QuerySelector("span.small.fade")?.TextContent ?? "", @"\d+");
                    int.TryParse(cell.Id.AsSpan(2), out var id);
                    return new ReplyModel
                    {
                        Id = id,
                        Avater = cell.QuerySelector("img.avatar")?.GetAttribute("src")
                                 ?? cell.QuerySelector("img")?.GetAttribute("src"),
                        Username = username,
                        Content = cell.QuerySelector("div.reply_content")?.InnerHtml.Trim() ?? "",
                        Thanks = thanksMatch.Success ? int.Parse(thanksMatch.Value) : 0,
                        Floor = $"#{cell.QuerySelector("span.no")?.TextContent}",
                        ReplyDate = cell.QuerySelector("span.ago")?.TextContent?.Trim() ?? "",
                        IsLz = lzUsername == username,
                        IsThanked = cell.QuerySelector("div.thank_area")?.ClassList.Contains("thanked") ?? false
                    };
                });
                return new PagesBaseModel<ReplyModel>
                {
                    Pages = Topic.Replies % 100 == 0 ? Topic.Replies / 100 : Topic.Replies / 100 + 1,
                    Entity = replies
                };
            };
        }

        private IncrementalLoadingCollection<ReplyModel> Replies { get; } = new IncrementalLoadingCollection<ReplyModel>();

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

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            var parameter = e.Parameter;
            if (parameter is int i)
            {
                _id = i;
            }
            else if (parameter is Tuple<int, int> tuple)
            {
                _id = tuple.Item1;
                _replyfloor = tuple.Item2;
            }
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void AuthorButton_Click(object sender, RoutedEventArgs e)
        {
            var username = (sender as FrameworkElement)?.Tag as string;
            if (!string.IsNullOrEmpty(username))
                PageStack.Next("Right", "Right", typeof(MemberView), username);
        }

        private void UsernameButton_Click(object sender, RoutedEventArgs e)
        {
            InsertMention((sender as FrameworkElement)?.Tag as string);
        }

        private void ReplyToButton_Click(object sender, RoutedEventArgs e)
        {
            InsertMention((sender as FrameworkElement)?.Tag as string);
        }

        private void InsertMention(string username)
        {
            if (string.IsNullOrEmpty(username)) return;
            var mention = $"@{username} ";
            if (!ReplyText.Text.Contains(mention))
            {
                if (!string.IsNullOrEmpty(ReplyText.Text) && !ReplyText.Text.EndsWith(" ") && !ReplyText.Text.EndsWith("\n"))
                    ReplyText.Text += " ";
                ReplyText.Text += mention;
            }
            ReplyText.Focus(FocusState.Programmatic);
            ReplyText.SelectionStart = ReplyText.Text.Length;
        }

        private void NodeButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(Topic.NodeSlug)) return;
            PageStack.Next("Right", "Right", typeof(OneNodeTopicsView), new NodeModel
            {
                Name = Topic.NodeSlug,
                Title = Topic.NodeName
            });
        }

        private async void CollectButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(Topic.Title)) return;
            CollectedPanel.IsEnabled = false;
            try
            {
                var html = await ApiClient.GetTopicInformation(_id);
                var url = "";
                var regexFav = new Regex("<a href=\"([^\"]*)\"[^>]*>加入收藏</a>");
                var regexUnFav = new Regex("<a href=\"([^\"]*)\"[^>]*>取消收藏</a>");
                if (regexFav.IsMatch(html)) url = regexFav.Match(html).Groups[1].Value;
                if (regexUnFav.IsMatch(html)) url = regexUnFav.Match(html).Groups[1].Value;
                if (string.IsNullOrEmpty(url)) return;
                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri($"https://www.v2ex.com{url}"),
                    Headers = { { "Referer", $"https://www.v2ex.com/t/{_id}" } }
                };
                await ApiClient.Client.SendAsync(request);
                Topic.IsCollected = !Topic.IsCollected;
                Topic.Collect = Topic.IsCollected ? "已收藏" : "收藏";
                Toast.ShowTips(Topic.IsCollected ? "收藏成功" : "取消收藏成功");
                OnPropertyChanged(nameof(Topic));
            }
            catch
            {
                Toast.ShowTips("操作失败");
            }
            finally
            {
                CollectedPanel.IsEnabled = true;
            }
        }

        private void CopyLinkButton_Click(object sender, RoutedEventArgs e)
        {
            var url = string.IsNullOrEmpty(Topic.Url) ? $"https://www.v2ex.com/t/{_id}" : Topic.Url;
            var clipBoardText = new DataPackage();
            clipBoardText.SetText(url);
            Clipboard.SetContent(clipBoardText);
            Toast.ShowTips("已复制到剪贴板");
        }

        private async void ThanksButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var button = sender as Button;
                var reply = button?.Tag as ReplyModel;
                if (reply == null) return;

                if (reply.IsThanked)
                {
                    Toast.ShowTips("已经感谢过这条回复了");
                    return;
                }

                var dialog = new ContentDialog
                {
                    Title = "确认",
                    Content = $"确认花费 10 个铜币向 @{reply.Username} 的这条回复发送感谢？",
                    PrimaryButtonText = "确认",
                    CloseButtonText = "取消",
                    XamlRoot = XamlRoot
                };
                var result = await dialog.ShowAsync();
                if (result != ContentDialogResult.Primary) return;

                var html = await ApiClient.GetTopicInformation(_id);
                var once = new HtmlParser().ParseDocument(html).QuerySelector("input[name='once']").GetAttribute("value");

                var error = await ApiClient.ThankReply(reply.Id, once);
                if (error == null)
                {
                    Toast.ShowTips("感谢已发送");
                    reply.Thanks += 1;
                    reply.IsThanked = true;
                }
                else
                {
                    Toast.ShowTips(error);
                }
            }
            catch
            {
                Toast.ShowTips("感谢失败");
            }
        }
    }
}
