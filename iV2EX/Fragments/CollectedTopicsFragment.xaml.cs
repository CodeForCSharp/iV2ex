using System;
using System.Collections.ObjectModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using iV2EX.GetData;
using iV2EX.Model;
using iV2EX.TupleModel;
using iV2EX.Util;
using iV2EX.Views;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using AngleSharp.Html.Parser;

namespace iV2EX.Fragments
{
    public partial class CollectedTopicsFragment
    {
        private int _loadGeneration;
        private CancellationTokenSource _loadCts;
        private CollectedListModel _selectedTab;
        private readonly Action _loadTopicsAction;

        private readonly CollectedListModel[] _tabs =
        {
            new CollectedListModel {Text = "技术", Name = "tech"},
            new CollectedListModel {Text = "创意", Name = "creative"},
            new CollectedListModel {Text = "好玩", Name = "play"},
            new CollectedListModel {Text = "Apple", Name = "apple"},
            new CollectedListModel {Text = "酷工作", Name = "jobs"},
            new CollectedListModel {Text = "交易", Name = "deals"},
            new CollectedListModel {Text = "城市", Name = "city"},
            new CollectedListModel {Text = "问与答", Name = "qna"},
            new CollectedListModel {Text = "最热", Name = "hot"},
            new CollectedListModel {Text = "全部", Name = "all"},
            new CollectedListModel {Text = "R2", Name = "r2"},
            new CollectedListModel {Text = "节点", Name = "nodes"},
            new CollectedListModel {Text = "关注", Name = "members"}
        };

        public CollectedTopicsFragment()
        {
            InitializeComponent();
            LabelPanel.ItemsSource = _tabs;

            async Task<IEnumerable<TopicModel>> loadData(string tabName)
            {
                var html = await ApiClient.GetTopicsWithTab(tabName);
                var dom = new HtmlParser().ParseDocument(html);
                return DomParse.ParseTopics(dom);
            }

            async void LoadTopics()
            {
                if (_selectedTab == null) return;

                var generation = ++_loadGeneration;
                _loadCts?.Cancel();
                _loadCts?.Dispose();
                _loadCts = new CancellationTokenSource();

                ErrorPanel.Visibility = Visibility.Collapsed;
                if (News.Count == 0)
                {
                    LoadingRing.IsActive = true;
                    LoadingRing.Visibility = Visibility.Visible;
                    LoadingBar.Visibility = Visibility.Collapsed;
                }
                else
                {
                    LoadingBar.Visibility = Visibility.Visible;
                }

                try
                {
                    var topics = await AsyncHelper.RetryAsync(() => loadData(_selectedTab.Name), 3);
                    if (generation != _loadGeneration) return;

                    News.Clear();
                    if (topics != null)
                    {
                        foreach (var item in topics)
                            News.Add(item);
                    }
                }
                catch
                {
                    if (generation != _loadGeneration) return;
                    if (News.Count == 0)
                        ErrorPanel.Visibility = Visibility.Visible;
                    else
                        Toast.ShowTips("刷新失败，请稍后重试");
                }
                finally
                {
                    if (generation == _loadGeneration)
                    {
                        LoadingRing.IsActive = false;
                        LoadingRing.Visibility = Visibility.Collapsed;
                        LoadingBar.Visibility = Visibility.Collapsed;
                    }
                }
            }

            _loadTopicsAction = LoadTopics;

            RetryButton.Click += (s, e) => { LoadTopics(); };

            NewsList.ItemClick += (s, e) =>
            {
                if (e.ClickedItem is TopicModel item)
                    PageStack.Next("Left", "Right", typeof(RepliesAndTopicView), item.Id);
            };

            SelectTab(_tabs[0]);
        }

        public ObservableCollection<TopicModel> News { get; } = new ObservableCollection<TopicModel>();

        private void SelectTab(CollectedListModel tab)
        {
            if (tab == null || _selectedTab == tab) return;
            if (_selectedTab != null)
                _selectedTab.IsSelected = false;
            _selectedTab = tab;
            _selectedTab.IsSelected = true;
            _loadTopicsAction?.Invoke();
        }

        private void TabButton_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as FrameworkElement)?.Tag is CollectedListModel tab)
                SelectTab(tab);
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            _loadTopicsAction?.Invoke();
        }
    }
}
