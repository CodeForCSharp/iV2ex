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
                var totalwidth = LeftPivot.ActualWidth;
                headerpanel[0].Width = totalwidth;
                var items = FindVisualChildren<PivotHeaderItem>(headerpanel[0]).ToList();
                for (var i = 0; i < items.Count; i++)
                    items[i].Width = totalwidth / items.Count - 1;
            };

            RightFrame.SizeChanged += (s, e) =>
            {
                if (RootGrid.ActualWidth > 600)
                {
                    LeftPivot.Width = 500;
                    LeftPivot.Visibility = Visibility.Visible;
                    RightFrame.Visibility = Visibility.Visible;
                    RightFrame.SetValue(RelativePanel.RightOfProperty, LeftPivot);
                    LeftPivot.SetValue(RelativePanel.AlignBottomWithPanelProperty, true);
                    LeftPivot.SetValue(RelativePanel.AlignTopWithPanelProperty, true);
                    LeftPivot.SetValue(RelativePanel.AlignLeftWithPanelProperty, true);
                    LeftPivot.SetValue(RelativePanel.AlignRightWithPanelProperty, false);
                    RightFrame.SetValue(RelativePanel.AlignBottomWithPanelProperty, true);
                    RightFrame.SetValue(RelativePanel.AlignTopWithPanelProperty, true);
                    RightFrame.SetValue(RelativePanel.AlignRightWithPanelProperty, true);
                    RightFrame.SetValue(RelativePanel.AlignLeftWithPanelProperty, false);
                }
                else
                {
                    LeftPivot.Width = double.NaN;
                    if (RightFrame.CanGoBack)
                    {
                        LeftPivot.Visibility = Visibility.Visible;
                        RightFrame.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        LeftPivot.Visibility = Visibility.Collapsed;
                        RightFrame.Visibility = Visibility.Visible;
                    }

                    LeftPivot.SetValue(RelativePanel.AlignBottomWithPanelProperty, true);
                    LeftPivot.SetValue(RelativePanel.AlignTopWithPanelProperty, true);
                    LeftPivot.SetValue(RelativePanel.AlignLeftWithPanelProperty, true);
                    LeftPivot.SetValue(RelativePanel.AlignRightWithPanelProperty, true);
                    RightFrame.SetValue(RelativePanel.AlignBottomWithPanelProperty, true);
                    RightFrame.SetValue(RelativePanel.AlignTopWithPanelProperty, true);
                    RightFrame.SetValue(RelativePanel.AlignRightWithPanelProperty, true);
                    RightFrame.SetValue(RelativePanel.AlignLeftWithPanelProperty, true);
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
