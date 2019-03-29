using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Reactive.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using iV2EX.GetData;
using iV2EX.Model;
using iV2EX.Util;
using AngleSharp.Html.Parser;

namespace iV2EX.Views
{
    public partial class WriteTopicView
    {

        private List<NodeModel> _nodes = new List<NodeModel>();

        private ObservableCollection<NodeModel> Show { get; } = new ObservableCollection<NodeModel>();

        private List<IDisposable> _events;
        public WriteTopicView()
        {
            InitializeComponent();
            var controls = new List<Control> {Option, Send, Title, Body};
            var load = Observable.FromEventPattern<RoutedEventArgs>(WrittenPage, nameof(WrittenPage.Loaded))
                .SelectMany(x => ApiClient.GetNodes())
                .Retry(10)
                .Subscribe(x => _nodes.AddRange(x));
            var wrriten = Observable.FromEventPattern<TappedRoutedEventArgs>(Send, nameof(Send.Tapped))
                .Select(async x =>
                {
                    if (Title.Text.Length == 0) return WrritenStatus.TitleEmpty;
                    if (Title.Text.Length > 120) return WrritenStatus.TitleLonger;
                    if (Body.Text.Length > 20000) return WrritenStatus.BodyLonger;
                    if (!_nodes.Exists(node => node.Title.Contains(Option.Text))) return WrritenStatus.NotExistNode;
                    var url = $"https://www.v2ex.com/new/{Option.Text}";
                    var html = await ApiClient.OnlyGet(url);
                    var once = new HtmlParser().ParseDocument(html).QuerySelector("input[name='once']").GetAttribute("value");
                    var param = new Dictionary<string, string>
                    {
                        {"once", once},
                        {"content", Body.Text},
                        {"title", Title.Text}
                    };
                    await ApiClient.NewTopic(url, new FormUrlEncodedContent(param), Option.Text);
                    return WrritenStatus.Success;
                })
                .Subscribe(async x =>
                {
                    controls.ForEach(y => y.IsEnabled = false);
                    try
                    {
                        switch (await x)
                        {
                            case WrritenStatus.NotExistNode:
                                Toast.ShowTips("节点不存在");
                                break;
                            case WrritenStatus.TitleEmpty:
                                Toast.ShowTips("标题字数不能为0");
                                break;
                            case WrritenStatus.TitleLonger:
                                Toast.ShowTips("标题字数不能超过120");
                                break;
                            case WrritenStatus.BodyLonger:
                                Toast.ShowTips("正文字数不能超过20000");
                                break;
                            case WrritenStatus.Success:
                                Toast.ShowTips("发表成功");
                                PageStack.Back();
                                break;
                        }
                    }
                    catch
                    {
                        Toast.ShowTips("发表失败");
                    }
                });
            var type = Observable
                .FromEventPattern<AutoSuggestBoxTextChangedEventArgs>(Option, nameof(Option.TextChanged))
                .Throttle(TimeSpan.FromMilliseconds(300))
                .Select(x => _nodes.Where(node => node.Title.Contains(Option.Text)))
                .ObserveOnDispatcher()
                .Subscribe(x =>
                {
                    foreach (var node in x)
                        Show.Add(node);
                });
            var choose = Observable
                .FromEventPattern<AutoSuggestBoxSuggestionChosenEventArgs>(Option, nameof(Option.SuggestionChosen))
                .ObserveOnDispatcher()
                .Subscribe(x => Option.Text = (x.EventArgs.SelectedItem as NodeModel).Title);

            _events = new List<IDisposable> { load, wrriten, type, choose };
        }

        protected internal override void OnDestroy()
        {
            base.OnDestroy();
            _events.ForEach(x => x.Dispose());
        }
    }
}