using System;
using System.Linq;
using System.Reactive.Linq;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using iV2EX.GetData;
using iV2EX.Model;
using iV2EX.Util;
using iV2EX.Views;
using System.Reactive.Concurrency;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace iV2EX.Fragments
{
    public partial class NodesFragment
    {
        public NodesFragment()
        {
            InitializeComponent();
            var loaded = Observable.FromEventPattern<RoutedEventArgs>(AllNodesFragment, nameof(AllNodesFragment.Loaded))
                .SelectMany(x => ApiClient.GetNodes())
                .Retry(10)
                .ObserveOn(DispatcherQueueScheduler.Current)
                .Subscribe(x =>
                {
                    InView.ItemsSource = x;
                });
            var click = Observable.FromEventPattern<ItemClickEventArgs>(InView, nameof(InView.ItemClick))
                .Select(x => x.EventArgs.ClickedItem as NodeModel)
                .ObserveOn(DispatcherQueueScheduler.Current)
                .Subscribe(x => PageStack.Next("Left", "Right", typeof(OneNodeTopicsView), x));
            this.Unloaded += (s, e) =>
            {
                loaded.Dispose();
                click.Dispose();
            };
        }
    }
}