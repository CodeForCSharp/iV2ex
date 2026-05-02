using System.Collections.Generic;
using System.Linq;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Media;
using iV2EX.Util;

namespace iV2EX.Views
{
    internal partial class MainPage
    {
        public MainPage()
        {
            InitializeComponent();
            RightPart = RightFrame;
            LeftPart = LeftPivot;
            PageStack.Next("Right", "Right", typeof(BlankPage), null);

            LeftPivot.SizeChanged += (s, e) =>
            {
                var headerpanel = FindVisualChildren<PivotHeaderPanel>(LeftPivot).ToList();
                if (headerpanel.Count == 0) return;
                var totalwidth = LeftPivot.ActualWidth;
                headerpanel[0].Width = totalwidth;
                var items = FindVisualChildren<PivotHeaderItem>(headerpanel[0]).ToList();
                for (var i = 0; i < items.Count; i++)
                    items[i].Width = totalwidth / items.Count - 1;
            };

            RootGrid.SizeChanged += (s, e) =>
            {
                if (RootGrid.ActualWidth <= 600 && e.PreviousSize.Width > 600)
                {
                    if (PageStack.IsLeftToRightActive)
                    {
                        LeftPivot.Visibility = Visibility.Collapsed;
                        RightFrame.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        LeftPivot.Visibility = Visibility.Visible;
                        RightFrame.Visibility = Visibility.Collapsed;
                    }
                }
            };
        }

        public static Frame RightPart { get; private set; }
        public static Pivot LeftPart { get; private set; }

        private static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
                for (var i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    var child = VisualTreeHelper.GetChild(depObj, i);
                    if (child is T variable)
                        yield return variable;

                    foreach (var childOfChild in FindVisualChildren<T>(child))
                        yield return childOfChild;
                }
        }
    }
}
