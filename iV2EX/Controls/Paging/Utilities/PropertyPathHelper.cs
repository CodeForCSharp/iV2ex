//-----------------------------------------------------------------------
// <copyright file="PropertyPathHelper.cs" company="MyToolkit">
//     Copyright (c) Rico Suter. All rights reserved.
// </copyright>
// <license>https://github.com/MyToolkit/MyToolkit/blob/master/LICENSE.md</license>
// <author>Rico Suter, mail@rsuter.com</author>
//-----------------------------------------------------------------------

using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using MyToolkit.UI;

namespace MyToolkit.Utilities
{
    /// <summary>Provides helper methods for handling property paths. </summary>
    public static class PropertyPathHelper
    {
        private static readonly DependencyProperty DummyProperty =
            DependencyProperty.RegisterAttached(
                "Dummy", typeof(object),
                typeof(DependencyObject),
                new PropertyMetadata(null));

        public static object Evaluate(object container, Binding binding)
        {
            DependencyObject dummyDO = new MyDependencyObject();
            BindingOperations.SetBinding(dummyDO, DummyProperty, binding);
            return dummyDO.GetValue(DummyProperty);
        }

        public static object Evaluate(object container, PropertyPath propertyPath)
        {
            return Evaluate(container, new Binding {Source = container, Path = propertyPath});
        }

        public static object Evaluate(object container, string propertyPath)
        {
            return Evaluate(container, new PropertyPath(propertyPath));
        }
    }
}