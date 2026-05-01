using System;
using System.IO;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.Streams;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Imaging;
using iV2EX.GetData;

namespace iV2EX.Views
{
    public sealed partial class ImageViewerPage
    {
        private StorageFile _file;
        private string _imageUrl;
        private bool _isDragging;
        private Point _dragStartPoint;
        private double _dragStartHOffset;
        private double _dragStartVOffset;

        public ImageViewerPage()
        {
            InitializeComponent();

            ImageScrollViewer.PointerPressed += ImageScrollViewer_PointerPressed;
            ImageScrollViewer.PointerMoved += ImageScrollViewer_PointerMoved;
            ImageScrollViewer.PointerReleased += ImageScrollViewer_PointerReleased;
        }

        private void WrapPanel_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ImageContainer.Width = e.NewSize.Width;
            ImageContainer.Height = e.NewSize.Height;
        }

        private void ImageScrollViewer_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            _isDragging = true;
            _dragStartPoint = e.GetCurrentPoint(ImageScrollViewer).Position;
            _dragStartHOffset = ImageScrollViewer.HorizontalOffset;
            _dragStartVOffset = ImageScrollViewer.VerticalOffset;
            ImageScrollViewer.CapturePointer(e.Pointer);
        }

        private void ImageScrollViewer_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (!_isDragging) return;
            var position = e.GetCurrentPoint(ImageScrollViewer).Position;
            var deltaX = _dragStartPoint.X - position.X;
            var deltaY = _dragStartPoint.Y - position.Y;
            ImageScrollViewer.ChangeView(
                _dragStartHOffset + deltaX,
                _dragStartVOffset + deltaY,
                null);
        }

        private void ImageScrollViewer_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            _isDragging = false;
            ImageScrollViewer.ReleasePointerCapture(e.Pointer);
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
                ImageScrollViewer.ChangeView(null, null, 1.0f);

                try
                {
                    using var stream = await ApiClient.GetStream(_imageUrl);
                    var extension = Path.GetExtension(new Uri(_imageUrl).AbsolutePath);
                    if (string.IsNullOrEmpty(extension))
                        extension = ".png";
                    _file = await ApplicationData.Current.TemporaryFolder.CreateFileAsync(
                        $"iv2ex_image{extension}", CreationCollisionOption.ReplaceExisting);
                    using var fileStream = await _file.OpenStreamForWriteAsync();
                    await stream.CopyToAsync(fileStream);
                }
                catch
                {
                    // download failed, clipboard/save will show error toast
                }
            }
        }

        private void ImageScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            var zoom = ImageScrollViewer.ZoomFactor;
            ZoomPercentageText.Text = $"{zoom * 100:0}%";
        }

        private void ZoomInButton_Click(object sender, RoutedEventArgs e)
        {
            var current = (double)ImageScrollViewer.ZoomFactor;
            var newZoom = (float)Math.Min(current + 0.1f, ImageScrollViewer.MaxZoomFactor);
            ImageScrollViewer.ChangeView(null, null, newZoom);
        }

        private void ZoomOutButton_Click(object sender, RoutedEventArgs e)
        {
            var current = (double)ImageScrollViewer.ZoomFactor;
            var newZoom = (float)Math.Max(current - 0.1f, ImageScrollViewer.MinZoomFactor);
            ImageScrollViewer.ChangeView(null, null, newZoom);
        }

        private async void CopyButton_Click(object sender, RoutedEventArgs e)
        {
            if (_file == null)
            {
                ToastTips.ShowTips("图片未加载完成");
                return;
            }
            try
            {
                var dataPackage = new DataPackage();
                dataPackage.SetBitmap(RandomAccessStreamReference.CreateFromFile(_file));
                Clipboard.SetContent(dataPackage);
                ToastTips.ShowTips("已复制到剪贴板");
            }
            catch
            {
                ToastTips.ShowTips("复制失败");
            }
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (_file == null)
            {
                ToastTips.ShowTips("图片未加载完成");
                return;
            }
            try
            {
                var library = await StorageLibrary.GetLibraryAsync(KnownLibraryId.Pictures);
                var path = await library.SaveFolder.CreateFolderAsync("iV2EX",
                    CreationCollisionOption.OpenIfExists);
                await _file.CopyAsync(path, _file.Name, NameCollisionOption.ReplaceExisting);
                ToastTips.ShowTips("已经保存到图片库");
            }
            catch
            {
                ToastTips.ShowTips("保存失败");
            }
        }
    }
}
