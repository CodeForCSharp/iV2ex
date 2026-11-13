using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using iV2EX.GetData;
using iV2EX.Model;
using iV2EX.Util;
using iV2EX.Views;

namespace iV2EX.Fragments
{
    public partial class NodesFragment
    {
        private List<NodeModel> _allNodes;
        private bool _isLoading;
        private readonly Action<string> _debouncedFilter;

        public NodesFragment()
        {
            InitializeComponent();
            _debouncedFilter = AsyncHelper.Debounce<string>(ApplyFilter, TimeSpan.FromMilliseconds(200));

            AllNodesFragment.Loaded += async (s, e) =>
            {
                if (_allNodes != null) return;
                await LoadNodesAsync();
            };

            RetryButton.Click += async (s, e) =>
            {
                await LoadNodesAsync();
            };

            InView.ItemClick += (s, e) =>
            {
                if (e.ClickedItem is NodeModel item)
                    PageStack.Next("Left", "Right", typeof(OneNodeTopicsView), item);
            };
        }

        private async Task LoadNodesAsync()
        {
            if (_isLoading) return;
            _isLoading = true;
            ErrorPanel.Visibility = Visibility.Collapsed;
            EmptyHint.Visibility = Visibility.Collapsed;
            LoadingRing.IsActive = true;
            LoadingRing.Visibility = Visibility.Visible;

            try
            {
                _allNodes = await AsyncHelper.RetryAsync(() => ApiClient.GetNodes(), 3);
                ApplyFilter(SearchBox.Text);
            }
            catch
            {
                ErrorPanel.Visibility = Visibility.Visible;
            }
            finally
            {
                _isLoading = false;
                LoadingRing.IsActive = false;
                LoadingRing.Visibility = Visibility.Collapsed;
            }
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            _debouncedFilter(SearchBox.Text);
        }

        private void ApplyFilter(string keyword)
        {
            if (_allNodes == null) return;

            var query = keyword?.Trim();
            var filtered = string.IsNullOrEmpty(query)
                ? _allNodes
                : _allNodes.Where(n =>
                    (!string.IsNullOrEmpty(n.Title) && n.Title.Contains(query, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrEmpty(n.Name) && n.Name.Contains(query, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrEmpty(n.Header) && n.Header.Contains(query, StringComparison.OrdinalIgnoreCase)))
                    .ToList();

            InView.ItemsSource = filtered;
            EmptyHint.Visibility = filtered.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}
