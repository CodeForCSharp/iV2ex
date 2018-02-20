using System;
using System.IO;
using System.Reactive.Linq;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;
using Microsoft.Toolkit.Uwp.UI;
using MyToolkit.Paging;

namespace iV2EX.Views
{
    public sealed partial class ImageViewerPage
    {
        private StorageFile _file;
        private string _imageUrl;

        public ImageViewerPage()
        {
            InitializeComponent();
            DataTransferManager.GetForCurrentView().DataRequested += (s, e) =>
            {
                e.Request.Data.Properties.Title = "分享自iV2EX";
                e.Request.Data.SetText(_imageUrl);
                e.Request.Data.SetBitmap(RandomAccessStreamReference.CreateFromFile(_file));
            };
            var share = Observable.FromEventPattern<RoutedEventArgs>(ShareImage, nameof(ShareImage.Click))
                .ObserveOnDispatcher()
                .Subscribe(x =>
                {
                    if (DataTransferManager.IsSupported()) DataTransferManager.ShowShareUI();
                });
            var save = Observable.FromEventPattern<RoutedEventArgs>(SaveImage, nameof(SaveImage.Click))
                .ObserveOnDispatcher()
                .Subscribe(async x =>
                {
                    try
                    {
                        var library = await StorageLibrary.GetLibraryAsync(KnownLibraryId.Pictures);
                        var path = await library.SaveFolder.CreateFolderAsync("iV2EX",
                            CreationCollisionOption.OpenIfExists);
                        await _file.CopyAsync(path, _file.Name + Path.GetExtension(_imageUrl),
                            NameCollisionOption.ReplaceExisting);
                        ToastTips.ShowTips("已经保存到图片库");
                    }
                    catch
                    {
                        ToastTips.ShowTips("保存失败");
                    }
                });
            var menu = Observable.FromEventPattern<TappedRoutedEventArgs>(MenuItemPanel, nameof(MenuItemPanel.Tapped))
                .ObserveOnDispatcher()
                .Subscribe(x => MenuItemPanel.ContextFlyout.ShowAt(MenuItemPanel));
        }

        protected internal override async void OnNavigatedTo(MtNavigationEventArgs args)
        {
            _imageUrl = args.GetParameter<string>();
            ImagePanel.Source = _imageUrl;
            await ImageCache.Instance.PreCacheAsync(new Uri(_imageUrl));
            _file = await ImageCache.Instance.GetFileFromCacheAsync(new Uri(_imageUrl));
            base.OnNavigatedTo(args);
        }
    }
}