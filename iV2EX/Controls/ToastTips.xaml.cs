using System;
using Microsoft.UI.Xaml;

namespace iV2EX.Controls
{
    public sealed partial class ToastTips
    {
        public ToastTips()
        {
            InitializeComponent();
        }

        public void ShowTips(string content)
        {
            Container.Text = content;
            ToastTransform.TranslateY = -8;
            ToastRoot.Visibility = Visibility.Visible;
            var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(1500) };
            timer.Tick += (s, e) =>
            {
                ToastRoot.Visibility = Visibility.Collapsed;
                timer.Stop();
            };
            timer.Start();
        }
    }
}
