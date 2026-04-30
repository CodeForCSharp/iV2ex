using System;
using System.Collections.ObjectModel;
using Microsoft.UI.Xaml.Controls;
using iV2EX.GetData;
using iV2EX.Model;
using iV2EX.TupleModel;
using iV2EX.Util;
using iV2EX.Views;
using System.Threading.Tasks;
using System.Collections.Generic;
using AngleSharp.Html.Parser;

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
            async Task<IEnumerable<TopicModel>> loadData()
            {
                var model = LabelPanel.SelectedItem as CollectedListModel;
                var html = await ApiClient.GetTopicsWithTab(model.Name);
                var dom = new HtmlParser().ParseDocument(html);
                return DomParse.ParseTopics(dom);
            }

            async void LoadTopics()
            {
                var topics = await AsyncHelper.RetryAsync(() => loadData(), 5);
                News.Clear();
                foreach (var item in topics)
                    News.Add(item);
            }

            LabelPanel.SelectionChanged += (s, e) => { LoadTopics(); };
            Refresh.Tapped += (s, e) => { LoadTopics(); };

            NewsList.ItemClick += (s, e) =>
            {
                if (e.ClickedItem is TopicModel item)
                    PageStack.Next("Left", "Right", typeof(RepliesAndTopicView), item.Id);
            };

            LabelPanel.SelectedIndex = 0;
        }

        public ObservableCollection<TopicModel> News { get; } = new ObservableCollection<TopicModel>();
    }
}
