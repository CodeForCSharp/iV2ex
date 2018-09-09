using System;
using System.Collections.ObjectModel;
using System.Reactive.Linq;
using Windows.UI.Xaml.Controls;
using AngleSharp.Parser.Html;
using iV2EX.GetData;
using iV2EX.Model;
using iV2EX.TupleModel;
using iV2EX.Util;
using iV2EX.Views;
using Windows.UI.Xaml.Input;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace iV2EX.Fragments
{
    public partial class CollectedTopicsFragment
    {

        private CollectedListModel[] _tabs =
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
            var loadData = Observable.FromAsync(async x =>
            {
                var model = LabelPanel.SelectedItem as CollectedListModel;
                var html = await ApiClient.GetTopicsWithTab(model.Name);
                var dom = new HtmlParser().Parse(html);
                return DomParse.ParseTopics(dom);
            }).Retry(10);
            var selectionChanged = Observable
                .FromEventPattern<SelectionChangedEventArgs>(LabelPanel, nameof(LabelPanel.SelectionChanged))
                .SelectMany(x => loadData)
                .ObserveOnDispatcher()
                .Subscribe(x =>
                {
                    News.Clear();
                    foreach (var item in x)
                        News.Add(item);
                });
            var click = Observable.FromEventPattern<ItemClickEventArgs>(NewsList, nameof(NewsList.ItemClick))
                .ObserveOnDispatcher()
                .Subscribe(x =>
                {
                    var item = x.EventArgs.ClickedItem as TopicModel;
                    PageStack.Next("Left", "Right", typeof(RepliesAndTopicView), item.Id);
                });
            var refresh = Observable.FromEventPattern<TappedRoutedEventArgs>(Refresh, nameof(Refresh.Tapped))
                .SelectMany(x=>loadData)
                .ObserveOnDispatcher()
                .Subscribe(x =>
                {
                    News.Clear();
                    foreach (var item in x)
                        News.Add(item);
                });
            LabelPanel.SelectedIndex = 0;
        }

        public ObservableCollection<TopicModel> News { get; } = new ObservableCollection<TopicModel>();


    }
}