using System;
using System.Linq;
using System.Reactive.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using iV2EX.GetData;
using iV2EX.Model;
using iV2EX.TupleModel;
using iV2EX.Util;
using iV2EX.Views;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace iV2EX.Fragments
{
    public partial class NodesFragment
    {
        public NodesFragment()
        {
            InitializeComponent();
            var client = ApiClient.Client;
            var loadData = Observable.FromAsync(async x => await client.GetNodes())
                .Select(x =>
                {
                    return x.GroupBy(y =>
                        {
                            var begin = y.Title.Trim().First();
                            if (begin >= 'A' && begin <= 'Z')
                                return $"{begin}";
                            if (begin >= 'a' && begin <= 'z')
                                return $"{(char) (begin + 'A' - 'a')}";
                            if (begin >= 0 && begin <= 256)
                                return "0-9";
                            var first = PinYin.GetFirstPinyin(begin.ToString());
                            return string.IsNullOrEmpty(first) ? "其他" : first;
                        })
                        .Select(y => new NodeInGroup {Key = y.Key, NodeContent = y.ToList()})
                        .OrderBy(y => y.Key);
                }).Retry(10);
            var loaded = Observable.FromEventPattern<RoutedEventArgs>(AllNodesFragment, nameof(AllNodesFragment.Loaded))
                .Publish(x => loadData)
                .ObserveOnDispatcher()
                .Subscribe(x =>
                {
                    SortNodesCVS.Source = x;
                    OutView.ItemsSource = SortNodesCVS.View.CollectionGroups;
                    InView.ItemsSource = SortNodesCVS.View;
                });
            var click = Observable.FromEventPattern<ItemClickEventArgs>(InView, nameof(InView.ItemClick))
                .ObserveOnDispatcher()
                .Subscribe(x =>
                {
                    var item = x.EventArgs.ClickedItem as NodeModel;
                    PageStack.Next("Left", "Right", typeof(OneNodeTopicsView), item);
                });
        }
    }
}