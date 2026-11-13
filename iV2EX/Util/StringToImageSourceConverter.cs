using System;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media.Imaging;

namespace iV2EX.Util
{
    public partial class StringToImageSourceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is string url && !string.IsNullOrWhiteSpace(url))
            {
                url = url.Trim();
                if (url.StartsWith("//"))
                    url = "https:" + url;
                else if (url.StartsWith("/"))
                    url = "https://www.v2ex.com" + url;

                if (Uri.TryCreate(url, UriKind.Absolute, out var uri))
                    return new BitmapImage(uri);
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) =>
            throw new NotImplementedException();
    }
}
