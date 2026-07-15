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

namespace iV2EX.Views
{
    public partial class WriteTopicView
    {
        private static List<NodeModel> _cachedNodes;
        private static readonly List<NodeModel> _emptyNodes = new();
        private List<NodeModel> _nodes => _cachedNodes ?? _emptyNodes;
        private NodeModel _selectedNode;
        private string _currentSearchText = string.Empty;

        private ObservableCollection<NodeModel> Show { get; } = new();

        public WriteTopicView()
        {
            InitializeComponent();
            var controls = new List<Control> { Option, Send, TitleText, Body };

            WrittenPage.Loaded += async (s, e) =>
            {
                if (_cachedNodes != null) return;
                _cachedNodes = await AsyncHelper.RetryAsync(() => ApiClient.GetNodes(), 5);
            };

            Send.Tapped += async (s, e) =>
            {
                if (TitleText.Text.Length == 0) { Toast.ShowTips("标题字数不能为0"); return; }
                if (TitleText.Text.Length > 120) { Toast.ShowTips("标题字数不能超过120"); return; }
                if (Body.Text.Length > 20000) { Toast.ShowTips("正文字数不能超过20000"); return; }
                if (_selectedNode == null) { Toast.ShowTips("请先选择节点"); return; }

                controls.ForEach(y => y.IsEnabled = false);
                try
                {
                    var url = $"https://www.v2ex.com/new/{_selectedNode.Name}";
                    var html = await ApiClient.OnlyGet(url);
                    var once = new HtmlParser().ParseDocument(html)
                        .QuerySelector("input[name='once']").GetAttribute("value");
                    var param = new Dictionary<string, string>
                    {
                        { "once", once },
                        { "content", Body.Text },
                        { "title", TitleText.Text }
                    };
                    await ApiClient.NewTopic(url, new FormUrlEncodedContent(param), _selectedNode.Name);
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
                var filtered = string.IsNullOrEmpty(_currentSearchText)
                    ? _nodes
                    : _nodes.Where(node =>
                        node.Title.Contains(_currentSearchText, StringComparison.OrdinalIgnoreCase)
                        || node.Name.Contains(_currentSearchText, StringComparison.OrdinalIgnoreCase));
                Show.Clear();
                foreach (var node in filtered)
                    Show.Add(node);
            }, TimeSpan.FromMilliseconds(300));

            Option.TextChanged += (_, e) =>
            {
                _currentSearchText = Option.Text;
                debouncedSearch(e);
            };

            Option.SuggestionChosen += (s, e) =>
            {
                _selectedNode = e.SelectedItem as NodeModel;
                Option.Text = _selectedNode?.Title;
            };

            Option.GotFocus += (s, e) =>
            {
                if (string.IsNullOrEmpty(Option.Text))
                {
                    Show.Clear();
                    foreach (var node in _nodes)
                        Show.Add(node);
                }
            };

            Option.QuerySubmitted += (s, e) =>
            {
                if (e.ChosenSuggestion != null)
                {
                    _selectedNode = e.ChosenSuggestion as NodeModel;
                    Option.Text = _selectedNode?.Title;
                }
                else if (Show.Count == 1)
                {
                    _selectedNode = Show[0];
                    Option.Text = _selectedNode.Title;
                }
            };
        }
    }
}
