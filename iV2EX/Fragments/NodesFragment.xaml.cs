using System;
using iV2EX.GetData;
using iV2EX.Model;
using iV2EX.Util;
using iV2EX.Views;

namespace iV2EX.Fragments
{
    public partial class NodesFragment
    {
        public NodesFragment()
        {
            InitializeComponent();
            AllNodesFragment.Loaded += async (s, e) =>
            {
                InView.ItemsSource = await AsyncHelper.RetryAsync(() => ApiClient.GetNodes(), 5);
            };
            InView.ItemClick += (s, e) =>
            {
                if (e.ClickedItem is NodeModel item)
                    PageStack.Next("Left", "Right", typeof(OneNodeTopicsView), item);
            };
        }
    }
}
