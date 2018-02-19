//-----------------------------------------------------------------------
// <copyright file="DependencyPropertyHelper.cs" company="MyToolkit">
//     Copyright (c) Rico Suter. All rights reserved.
// </copyright>
// <license>https://github.com/MyToolkit/MyToolkit/blob/master/LICENSE.md</license>
// <author>Rico Suter, mail@rsuter.com</author>
//-----------------------------------------------------------------------

using System;
using System.Linq.Expressions;
using System.Reflection;
using Windows.UI.Xaml;
using MyToolkit.Utilities;

namespace MyToolkit.Mvvm
{
    public static class DependencyPropertyHelper
    {
        public static PropertyChangedCallback CallMethod<TView>(Action<TView> method) where TView : DependencyObject
        {
            return (obj, args) => method((TView) obj);
        }

        public static PropertyChangedCallback BindToViewModel<TViewModel>(Expression<Func<TViewModel, object>> property)
        {
            return BindToViewModel(ExpressionUtilities.GetPropertyName(property));
        }

        public static PropertyChangedCallback BindToViewModel(string propertyName)
        {
            return (obj, args) =>
            {
                var vm = ((FrameworkElement) obj).Resources["ViewModel"];
                vm.GetType().GetRuntimeProperty(propertyName).SetValue(vm, args.NewValue, null);
            };
        }
    }
}