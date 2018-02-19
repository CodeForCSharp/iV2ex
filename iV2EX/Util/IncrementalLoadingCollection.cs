using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml.Data;
using iV2EX.TupleModel;

namespace iV2EX.Util
{
    public class IncrementalLoadingCollection<T> : ObservableCollection<T>, ISupportIncrementalLoading
    {
        private bool _busy;
        public int MaxPage { get; set; }
        public int CurrentPage { get; private set; } = 1;
        public Func<int, Task<PagesBaseModel<T>>> LoadDataTask { get; set; }
        public bool HasMoreItems { get; private set; } = true;

        public IAsyncOperation<LoadMoreItemsResult> LoadMoreItemsAsync(uint count)
        {
            if (_busy) throw new InvalidOperationException("Only one operation in flight at a time");
            _busy = true;
            return AsyncInfo.Run(c => LoadMoreItemsAsync(c, count));
        }

        public async Task<LoadMoreItemsResult> LoadMoreItemsAsync(CancellationToken c, uint count)
        {
            try
            {
                var model = await LoadDataTask(Count);
                var items = model.Entity;
                MaxPage = model.Pages;
                foreach (var item in items) Add(item);
                CurrentPage++;
                HasMoreItems = CurrentPage <= MaxPage;
                return new LoadMoreItemsResult {Count = (uint) items.Count()};
            }
            catch
            {
                return new LoadMoreItemsResult {Count = 0};
            }
            finally
            {
                _busy = false;
            }
        }
    }
}