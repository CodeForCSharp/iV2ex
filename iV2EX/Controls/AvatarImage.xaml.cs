using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using iV2EX.GetData;
using iV2EX.Util;

namespace iV2EX.Controls
{
    public sealed partial class AvatarImage : UserControl
    {
        private const int MaxCacheEntries = 300;
        private static readonly ConcurrentDictionary<string, byte[]> BytesCache = new();
        private static readonly ConcurrentDictionary<string, Task<byte[]>> InFlightDownloads = new();

        private string _currentUrl;

        public static readonly DependencyProperty ImageUrlProperty =
            DependencyProperty.Register(
                nameof(ImageUrl),
                typeof(string),
                typeof(AvatarImage),
                new PropertyMetadata(null, OnImageUrlChanged));

        public static readonly DependencyProperty AvatarSizeProperty =
            DependencyProperty.Register(
                nameof(AvatarSize),
                typeof(double),
                typeof(AvatarImage),
                new PropertyMetadata(40d, OnAvatarSizeChanged));

        public AvatarImage()
        {
            InitializeComponent();
            UpdateSizeVisuals(AvatarSize);
        }

        public string ImageUrl
        {
            get => (string)GetValue(ImageUrlProperty);
            set => SetValue(ImageUrlProperty, value);
        }

        public double AvatarSize
        {
            get => (double)GetValue(AvatarSizeProperty);
            set => SetValue(AvatarSizeProperty, value);
        }

        private static void OnAvatarSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is AvatarImage control && e.NewValue is double size)
                control.UpdateSizeVisuals(size);
        }

        private static void OnImageUrlChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is AvatarImage control)
                _ = control.LoadAvatarAsync(e.NewValue as string);
        }

        private void UpdateSizeVisuals(double size)
        {
            if (size <= 0) return;
            AvatarBorder.Width = size;
            AvatarBorder.Height = size;
            AvatarBorder.CornerRadius = new CornerRadius(size / 2.0);
            PlaceholderIcon.FontSize = Math.Max(12, size * 0.42);
        }

        private static string NormalizeUrl(string rawUrl)
        {
            if (string.IsNullOrWhiteSpace(rawUrl)) return null;
            var url = rawUrl.Trim();
            if (url.StartsWith("//"))
                return "https:" + url;
            if (url.StartsWith("/"))
                return ApiClient.Host + url;
            return url;
        }

        private static Task<byte[]> GetOrDownloadBytesAsync(string url)
        {
            if (BytesCache.TryGetValue(url, out var cached))
                return Task.FromResult(cached);

            return InFlightDownloads.GetOrAdd(url, DownloadAsync);

            static async Task<byte[]> DownloadAsync(string targetUrl)
            {
                try
                {
                    var bytes = await ApiClient.Client.GetByteArrayAsync(targetUrl);
                    if (bytes is { Length: > 0 })
                    {
                        if (BytesCache.Count >= MaxCacheEntries)
                            BytesCache.Clear();
                        BytesCache[targetUrl] = bytes;
                        return bytes;
                    }
                    return null;
                }
                catch
                {
                    return null;
                }
                finally
                {
                    InFlightDownloads.TryRemove(targetUrl, out _);
                }
            }
        }

        private async Task LoadAvatarAsync(string rawUrl)
        {
            var normalizedUrl = NormalizeUrl(rawUrl);
            _currentUrl = normalizedUrl;

            AvatarImg.Source = null;
            PlaceholderIcon.Visibility = Visibility.Visible;

            if (string.IsNullOrEmpty(normalizedUrl) || !Uri.TryCreate(normalizedUrl, UriKind.Absolute, out var uri))
                return;

            if (uri.Scheme == "ms-appx")
            {
                AvatarImg.Source = new BitmapImage(uri);
                PlaceholderIcon.Visibility = Visibility.Collapsed;
                return;
            }

            var bytes = await GetOrDownloadBytesAsync(normalizedUrl);
            if (_currentUrl != normalizedUrl || bytes == null)
                return;

            try
            {
                var source = await GetBitmapFromUrl.GetBitmapFromBytes(bytes);
                if (_currentUrl != normalizedUrl || source == null)
                    return;

                AvatarImg.Source = source;
                PlaceholderIcon.Visibility = Visibility.Collapsed;
            }
            catch
            {
                if (_currentUrl == normalizedUrl)
                {
                    AvatarImg.Source = null;
                    PlaceholderIcon.Visibility = Visibility.Visible;
                }
            }
        }
    }
}
