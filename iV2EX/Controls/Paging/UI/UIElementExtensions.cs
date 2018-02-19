//-----------------------------------------------------------------------
// <copyright file="UIElementExtensions.cs" company="MyToolkit">
//     Copyright (c) Rico Suter. All rights reserved.
// </copyright>
// <license>https://github.com/MyToolkit/MyToolkit/blob/master/LICENSE.md</license>
// <author>Rico Suter, mail@rsuter.com</author>
//-----------------------------------------------------------------------

using Windows.UI.Xaml;

namespace MyToolkit.UI
{
    public static class UIElementExtensions
    {
        /// <summary>Use this attached property only to set the visibility.</summary>
        public static readonly DependencyProperty IsVisibleProperty =
            DependencyProperty.RegisterAttached("IsVisible", typeof(bool), typeof(FrameworkElementExtensions),
                new PropertyMetadata(true, IsVisibleChanged));

        public static readonly DependencyProperty IsEnabledProperty =
            DependencyProperty.RegisterAttached("IsEnabled", typeof(bool), typeof(UIElementExtensions),
                new PropertyMetadata(true, IsEnabledChanged));

        private static void IsVisibleChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var element = (UIElement) obj;
            element.Visibility = (bool) args.NewValue ? Visibility.Visible : Visibility.Collapsed;
        }

        public static void SetIsVisible(UIElement element, bool value)
        {
            element.SetValue(IsVisibleProperty, value);
        }

        public static bool GetIsVisible(UIElement element)
        {
            return (bool) element.GetValue(IsEnabledProperty);
        }

        private static void IsEnabledChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var element = (UIElement) obj;
            if ((bool) args.NewValue)
            {
                element.IsHitTestVisible = true;
                element.Opacity = 1.0;
            }
            else
            {
                element.IsHitTestVisible = false;
                element.Opacity = 0.5;
            }
        }

        public static void SetIsEnabled(UIElement element, bool value)
        {
            element.SetValue(IsEnabledProperty, value);
        }

        public static bool GetIsEnabled(UIElement element)
        {
            return (bool) element.GetValue(IsEnabledProperty);
        }
    }
}