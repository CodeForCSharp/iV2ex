using System;
using System.IO;
using System.Reactive.Linq;
using System.Runtime.InteropServices;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.Storage.Streams;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using System.Collections.Generic;
using Microsoft.UI.Xaml.Navigation;
using System.Reactive.Concurrency;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;

namespace iV2EX.Views
{
    public sealed partial class ImageViewerPage
    {
        private StorageFile _file;
        private string _imageUrl;
        private List<IDisposable> _events;

        public ImageViewerPage()
        {
            InitializeComponent();

            var share = Observable.FromEventPattern<RoutedEventArgs>(ShareImage, nameof(ShareImage.Click))
                .ObserveOn(DispatcherQueueScheduler.Current)
                .Subscribe(x =>
                {
                    if (DataTransferManager.IsSupported())
                    {
                        var interop = DataTransferManager.As<IDataTransferManagerInterop>();
                        var riid = typeof(DataTransferManager).GUID;
                        var manager = interop.GetForWindow(App.WindowHandle, riid);
                        manager.DataRequested += (s, e) =>
                        {
                            e.Request.Data.Properties.Title = "分享自iV2EX";
                            e.Request.Data.SetText(_imageUrl);
                            e.Request.Data.SetBitmap(RandomAccessStreamReference.CreateFromFile(_file));
                        };
                        interop.ShowShareUIForWindow(App.WindowHandle);
                    }
                });
            var save = Observable.FromEventPattern<RoutedEventArgs>(SaveImage, nameof(SaveImage.Click))
                .ObserveOn(DispatcherQueueScheduler.Current)
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
                .ObserveOn(DispatcherQueueScheduler.Current)
                .Subscribe(x => MenuItemPanel.ContextFlyout.ShowAt(MenuItemPanel));

            _events = new List<IDisposable> { share, save, menu };
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            _events.ForEach(x => x.Dispose());
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            var parameter = e.Parameter;
            _imageUrl = parameter as string;
            if (!string.IsNullOrEmpty(_imageUrl))
            {
                if (_imageUrl.StartsWith("//"))
                    _imageUrl = "https:" + _imageUrl;
                ImagePanel.Source = new BitmapImage(new Uri(_imageUrl));
            }
        }
    }

    [ComImport]
    [Guid("3A3DCD6C-3EAB-43DC-BCDE-45671CE800C8")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface IDataTransferManagerInterop
    {
        DataTransferManager GetForWindow(IntPtr appWindow, [In] ref Guid riid);
        void ShowShareUIForWindow(IntPtr appWindow);
    }
}
