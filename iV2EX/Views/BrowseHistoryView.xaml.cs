using System;
using System.Collections.ObjectModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using iV2EX.Model;
using iV2EX.Util;

namespace iV2EX.Views
{
    public partial class BrowseHistoryView
    {
        public BrowseHistoryView()
        {
            InitializeComponent();
        }

        public ObservableCollection<BrowseHistoryItem> Items { get; } = new();

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            var records = await BrowseHistoryStore.GetAllAsync();
            if (!IsLoaded) return;
            Items.Clear();
            foreach (var item in records)
                Items.Add(item);
            UpdateEmpty();
        }

        private void UpdateEmpty()
        {
            var empty = Items.Count == 0;
            EmptyHint.Visibility = empty ? Visibility.Visible : Visibility.Collapsed;
            ClearButton.Visibility = empty ? Visibility.Collapsed : Visibility.Visible;
        }

        private void HistoryList_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is BrowseHistoryItem item)
                PageStack.Next("Right", "Right", typeof(RepliesAndTopicView), item.Id);
        }

        private async void DeleteItem_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as FrameworkElement)?.Tag is not BrowseHistoryItem item) return;
            await BrowseHistoryStore.RemoveAsync(item.Id);
            Items.Remove(item);
            UpdateEmpty();
        }

        private async void Clear_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new ContentDialog
            {
                Title = "清空浏览历史",
                Content = "确定要清空全部浏览记录吗？",
                PrimaryButtonText = "清空",
                CloseButtonText = "取消",
                DefaultButton = ContentDialogButton.Close,
                XamlRoot = XamlRoot
            };
            if (await dialog.ShowAsync() != ContentDialogResult.Primary) return;

            await BrowseHistoryStore.ClearAsync();
            Items.Clear();
            UpdateEmpty();
        }
    }
}
