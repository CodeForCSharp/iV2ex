using System;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using iV2EX.GetData;

namespace iV2EX.Util
{
    internal class GetBitmapFromUrl
    {
        public static async Task<ImageSource> GetBitmapFromStream(string url)
        {
            using var inputStream = await ApiClient.GetStream(url);
            var memStream = new InMemoryRandomAccessStream();
            await RandomAccessStream.CopyAsync(inputStream.AsInputStream(), memStream);

            return await IsWebp(memStream)
                ? await LoadWebp(memStream)
                : await LoadBitmapImage(memStream);
        }

        private static async Task<ImageSource> LoadBitmapImage(IRandomAccessStream stream)
        {
            // BitmapImage decodes lazily (including GIF animation frames), so the stream must stay open.
            stream.Seek(0);
            var image = new BitmapImage();
            await image.SetSourceAsync(stream);
            return image;
        }

        private static async Task<ImageSource> LoadWebp(IRandomAccessStream stream)
        {
            using (stream)
            {
                stream.Seek(0);
                var decoder = await BitmapDecoder.CreateAsync(BitmapDecoder.WebpDecoderId, stream);
                var sb = await decoder.GetSoftwareBitmapAsync(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);
                var source = new SoftwareBitmapSource();
                await source.SetBitmapAsync(sb);
                return source;
            }
        }

        private static async Task<bool> IsWebp(IRandomAccessStream stream)
        {
            if (stream.Size < 12)
                return false;
            stream.Seek(0);
            var buffer = await stream.ReadAsync(new Windows.Storage.Streams.Buffer(12), 12, InputStreamOptions.None);
            var h = buffer.ToArray();
            return h.Length == 12
                   && h[0] == 'R' && h[1] == 'I' && h[2] == 'F' && h[3] == 'F'
                   && h[8] == 'W' && h[9] == 'E' && h[10] == 'B' && h[11] == 'P';
        }
    }
}
