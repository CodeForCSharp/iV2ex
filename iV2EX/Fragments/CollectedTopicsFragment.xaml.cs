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

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace iV2EX.Fragments
{
    public partial class CollectedTopicsFragment
    {
        public CollectedTopicsFragment()
        {
            InitializeComponent();
            var client = ApiClient.Client;
            LabelPanel.ItemsSource = V2ExManager.GetCollectedTab();
            var loadData = Observable.FromAsync(async x =>
            {
                var model = LabelPanel.SelectedItem as CollectedListModel;
                var html = await client.GetTopicsWithTab(model.Name);
                var dom = new HtmlParser().Parse(html);
                return V2ExManager.ParseTopics(dom);
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
            LabelPanel.SelectedIndex = 0;
        }

        public ObservableCollection<TopicModel> News { get; } = new ObservableCollection<TopicModel>();


    }
}