using System;
using System.IO;
using System.Runtime.InteropServices;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.Storage.Streams;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;

namespace iV2EX.Views
{
    public sealed partial class ImageViewerPage
    {
        private StorageFile _file;
        private string _imageUrl;

        public ImageViewerPage()
        {
            InitializeComponent();

            ShareImage.Click += (s, e) =>
            {
                if (DataTransferManager.IsSupported())
                {
                    var interop = DataTransferManager.As<IDataTransferManagerInterop>();
                    var riid = typeof(DataTransferManager).GUID;
                    var manager = interop.GetForWindow(App.WindowHandle, riid);
                    manager.DataRequested += (_, args) =>
                    {
                        args.Request.Data.Properties.Title = "分享自iV2EX";
                        args.Request.Data.SetText(_imageUrl);
                        args.Request.Data.SetBitmap(RandomAccessStreamReference.CreateFromFile(_file));
                    };
                    interop.ShowShareUIForWindow(App.WindowHandle);
                }
            };

            SaveImage.Click += async (s, e) =>
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
            };

            MenuItemPanel.Tapped += (s, e) =>
            {
                MenuItemPanel.ContextFlyout.ShowAt(MenuItemPanel);
            };
        }

        protected override async void OnNavigatedTo(Microsoft.UI.Xaml.Navigation.NavigationEventArgs e)
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
