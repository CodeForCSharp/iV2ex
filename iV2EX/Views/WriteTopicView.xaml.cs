using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using iV2EX.GetData;
using iV2EX.Model;
using iV2EX.Util;
using AngleSharp.Html.Parser;
using Microsoft.UI.Xaml.Navigation;

namespace iV2EX.Views
{
    public partial class WriteTopicView
    {

        private List<NodeModel> _nodes = new List<NodeModel>();

        private ObservableCollection<NodeModel> Show { get; } = new ObservableCollection<NodeModel>();

        public WriteTopicView()
        {
            InitializeComponent();
            var controls = new List<Control> {Option, Send, TitleText, Body};

            WrittenPage.Loaded += async (s, e) =>
            {
                _nodes.AddRange(await AsyncHelper.RetryAsync(() => ApiClient.GetNodes(), 5));
            };

            Send.Tapped += async (s, e) =>
            {
                if (TitleText.Text.Length == 0) { Toast.ShowTips("标题字数不能为0"); return; }
                if (TitleText.Text.Length > 120) { Toast.ShowTips("标题字数不能超过120"); return; }
                if (Body.Text.Length > 20000) { Toast.ShowTips("正文字数不能超过20000"); return; }
                if (!_nodes.Exists(node => node.Title.Contains(Option.Text))) { Toast.ShowTips("节点不存在"); return; }

                controls.ForEach(y => y.IsEnabled = false);
                try
                {
                    var url = $"https://www.v2ex.com/new/{Option.Text}";
                    var html = await ApiClient.OnlyGet(url);
                    var once = new HtmlParser().ParseDocument(html).QuerySelector("input[name='once']").GetAttribute("value");
                    var param = new Dictionary<string, string>
                    {
                        {"once", once},
                        {"content", Body.Text},
                        {"title", TitleText.Text}
                    };
                    await ApiClient.NewTopic(url, new FormUrlEncodedContent(param), Option.Text);
                    Toast.ShowTips("发表成功");
                    PageStack.Back();
                }
                catch
                {
                    Toast.ShowTips("发表失败");
                }
                finally
                {
                    controls.ForEach(y => y.IsEnabled = true);
                }
            };

            var debouncedSearch = AsyncHelper.Debounce<AutoSuggestBoxTextChangedEventArgs>(e =>
            {
                var filtered = _nodes.Where(node => node.Title.Contains(Option.Text));
                Show.Clear();
                foreach (var node in filtered)
                    Show.Add(node);
            }, TimeSpan.FromMilliseconds(300));

            Option.TextChanged += (_, e) => { debouncedSearch(e); };

            Option.SuggestionChosen += (s, e) =>
            {
                Option.Text = (e.SelectedItem as NodeModel)?.Title;
            };
        }
    }
}
