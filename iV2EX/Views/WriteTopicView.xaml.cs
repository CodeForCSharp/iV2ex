using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using AngleSharp.Html.Parser;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using iV2EX.GetData;
using iV2EX.Model;
using iV2EX.Util;

namespace iV2EX.Views
{
    public partial class WriteTopicView
    {
        private static List<NodeModel> _cachedNodes;
        private static readonly List<NodeModel> _emptyNodes = new();
        private List<NodeModel> _nodes => _cachedNodes ?? _emptyNodes;
        private NodeModel _selectedNode;
        private string _currentSearchText = string.Empty;
        private string _syntax = "default";
        private bool _isSubmitting;
        private readonly List<Control> _controls;

        private ObservableCollection<NodeModel> Show { get; } = new();

        public WriteTopicView()
        {
            InitializeComponent();
            _controls = new List<Control> { Option, Send, TitleText, Body, SyntaxDefaultRadio, SyntaxMarkdownRadio };

            WrittenPage.Loaded += async (s, e) =>
            {
                if (_cachedNodes != null) return;
                LoadingBar.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
                NodeStatusHint.Text = "正在加载节点列表...";
                try
                {
                    _cachedNodes = await AsyncHelper.RetryAsync(() => ApiClient.GetNodes(), 3);
                    NodeStatusHint.Text = _selectedNode == null ? "请选择节点后发布主题" : $"将发布到节点：{_selectedNode.Title}";
                }
                catch
                {
                    NodeStatusHint.Text = "节点列表加载失败，请返回重试";
                    Toast.ShowTips("节点列表加载失败");
                }
                finally
                {
                    LoadingBar.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
                }
            };

            var debouncedSearch = AsyncHelper.Debounce<AutoSuggestBoxTextChangedEventArgs>(e =>
            {
                var query = _currentSearchText?.Trim();
                var filtered = string.IsNullOrEmpty(query)
                    ? _nodes
                    : _nodes.Where(node =>
                        (!string.IsNullOrEmpty(node.Title) && node.Title.Contains(query, StringComparison.OrdinalIgnoreCase)) ||
                        (!string.IsNullOrEmpty(node.Name) && node.Name.Contains(query, StringComparison.OrdinalIgnoreCase)));
                Show.Clear();
                foreach (var node in filtered)
                    Show.Add(node);
            }, TimeSpan.FromMilliseconds(250));

            Option.TextChanged += (_, e) =>
            {
                _currentSearchText = Option.Text;
                if (e.Reason == AutoSuggestionBoxTextChangeReason.UserInput &&
                    _selectedNode != null &&
                    !string.Equals(Option.Text, _selectedNode.Title, StringComparison.Ordinal))
                {
                    SetSelectedNode(null);
                }
                debouncedSearch(e);
            };

            Option.SuggestionChosen += (s, e) =>
            {
                if (e.SelectedItem is NodeModel node)
                {
                    SetSelectedNode(node);
                    Option.Text = node.Title;
                }
            };

            Option.GotFocus += (s, e) =>
            {
                if (string.IsNullOrEmpty(Option.Text))
                {
                    Show.Clear();
                    foreach (var node in _nodes)
                        Show.Add(node);
                }
                Option.IsSuggestionListOpen = Show.Count > 0;
            };

            Option.QuerySubmitted += (s, e) =>
            {
                if (e.ChosenSuggestion is NodeModel chosen)
                {
                    SetSelectedNode(chosen);
                    Option.Text = chosen.Title;
                }
                else if (Show.Count == 1)
                {
                    SetSelectedNode(Show[0]);
                    Option.Text = Show[0].Title;
                }
            };
        }

        private void SetSelectedNode(NodeModel node)
        {
            _selectedNode = node;
            if (node == null)
            {
                SelectedNodeBadge.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
                SelectedNodeText.Text = string.Empty;
                NodeStatusHint.Text = "请选择节点后发布主题";
            }
            else
            {
                SelectedNodeText.Text = $"{node.Title} · {node.Name}";
                SelectedNodeBadge.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
                NodeStatusHint.Text = $"将发布到节点：{node.Title}";
            }
        }

        private void TitleText_TextChanged(object sender, TextChangedEventArgs e)
        {
            TitleCountText.Text = $"{TitleText.Text.Length} / 120";
        }

        private void Body_TextChanged(object sender, TextChangedEventArgs e)
        {
            BodyCountText.Text = $"{Body.Text.Length} / 20000";
        }

        private void SyntaxRadio_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is not RadioButton radio || radio.Tag is not string syntax) return;
            _syntax = syntax;
            if (Body != null)
            {
                Body.PlaceholderText = syntax == "markdown"
                    ? "使用 Markdown 语法编写正文（支持标题、代码块、列表与图片链接，可留空）..."
                    : "写下你的想法、问题描述或补充细节（自动识别链接与图片直链，可留空）...";
            }
        }

        private async void Send_Click(object sender, RoutedEventArgs e)
        {
            if (_isSubmitting) return;
            var title = TitleText.Text.Trim();
            if (title.Length == 0) { Toast.ShowTips("标题不能为空"); return; }
            if (title.Length > 120) { Toast.ShowTips("标题字数不能超过120"); return; }
            if (Body.Text.Length > 20000) { Toast.ShowTips("正文字数不能超过20000"); return; }
            if (_selectedNode == null) { Toast.ShowTips("请先选择节点"); return; }

            _isSubmitting = true;
            _controls.ForEach(c => c.IsEnabled = false);
            SendIcon.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
            SendRing.IsActive = true;
            SendRing.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
            SendButtonText.Text = "发布中...";

            try
            {
                var url = $"https://www.v2ex.com/new/{_selectedNode.Name}";
                var html = await ApiClient.OnlyGet(url);
                var once = new HtmlParser().ParseDocument(html)
                    .QuerySelector("input[name='once']")?.GetAttribute("value");
                if (string.IsNullOrEmpty(once))
                {
                    Toast.ShowTips("无法获取发帖凭证，请重试");
                    return;
                }

                var param = new Dictionary<string, string>
                {
                    { "once", once },
                    { "content", Body.Text },
                    { "title", title },
                    { "syntax", _syntax }
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
                _isSubmitting = false;
                SendRing.IsActive = false;
                SendRing.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
                SendIcon.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
                SendButtonText.Text = "发布主题";
                _controls.ForEach(c => c.IsEnabled = true);
            }
        }
    }
}
