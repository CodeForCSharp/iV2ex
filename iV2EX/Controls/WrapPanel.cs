using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Foundation;

namespace iV2EX.Controls
{
    public sealed partial class WrapPanel : Panel
    {
        public static readonly DependencyProperty HorizontalSpacingProperty =
            DependencyProperty.Register(nameof(HorizontalSpacing), typeof(double), typeof(WrapPanel),
                new PropertyMetadata(0d, OnLayoutPropertyChanged));

        public static readonly DependencyProperty VerticalSpacingProperty =
            DependencyProperty.Register(nameof(VerticalSpacing), typeof(double), typeof(WrapPanel),
                new PropertyMetadata(0d, OnLayoutPropertyChanged));

        public double HorizontalSpacing
        {
            get => (double)GetValue(HorizontalSpacingProperty);
            set => SetValue(HorizontalSpacingProperty, value);
        }

        public double VerticalSpacing
        {
            get => (double)GetValue(VerticalSpacingProperty);
            set => SetValue(VerticalSpacingProperty, value);
        }

        private static void OnLayoutPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is WrapPanel panel)
                panel.InvalidateMeasure();
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            var maxWidth = double.IsInfinity(availableSize.Width) ? double.MaxValue : availableSize.Width;
            double x = 0;
            double y = 0;
            double rowHeight = 0;
            double totalWidth = 0;

            foreach (var child in Children)
            {
                child.Measure(new Size(maxWidth, availableSize.Height));
                var desired = child.DesiredSize;
                var spacingX = x > 0 ? HorizontalSpacing : 0;

                if (x + spacingX + desired.Width > maxWidth && x > 0)
                {
                    totalWidth = Math.Max(totalWidth, x);
                    y += rowHeight + VerticalSpacing;
                    x = desired.Width;
                    rowHeight = desired.Height;
                }
                else
                {
                    x += spacingX + desired.Width;
                    rowHeight = Math.Max(rowHeight, desired.Height);
                }
            }

            totalWidth = Math.Max(totalWidth, x);
            return new Size(totalWidth, y + rowHeight);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            double x = 0;
            double y = 0;
            double rowHeight = 0;

            foreach (var child in Children)
            {
                var desired = child.DesiredSize;
                var spacingX = x > 0 ? HorizontalSpacing : 0;

                if (x + spacingX + desired.Width > finalSize.Width && x > 0)
                {
                    y += rowHeight + VerticalSpacing;
                    x = 0;
                    rowHeight = 0;
                }
                else
                {
                    x += spacingX;
                }

                child.Arrange(new Rect(x, y, desired.Width, desired.Height));
                x += desired.Width;
                rowHeight = Math.Max(rowHeight, desired.Height);
            }

            return finalSize;
        }
    }
}
