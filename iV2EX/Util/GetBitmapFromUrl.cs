using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;
using iV2EX.GetData;

namespace iV2EX.Util
{
    internal class GetBitmapFromUrl
    {
        public static async Task<SoftwareBitmapSource> GetBitmapFromStream(string url)
        {
            var inputStream = await ApiClient.GetStream(url);
            var memStream = new InMemoryRandomAccessStream();
            await RandomAccessStream.CopyAsync(inputStream.AsInputStream(), memStream);
            var decoder = await BitmapDecoder.CreateAsync(memStream);
            var sb = await decoder.GetSoftwareBitmapAsync(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);
            var source = new SoftwareBitmapSource();
            await source.SetBitmapAsync(sb);
            return source;
        }
    }
}