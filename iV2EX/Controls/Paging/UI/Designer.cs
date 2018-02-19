//-----------------------------------------------------------------------
// <copyright file="Designer.cs" company="MyToolkit">
//     Copyright (c) Rico Suter. All rights reserved.
// </copyright>
// <license>https://github.com/MyToolkit/MyToolkit/blob/master/LICENSE.md</license>
// <author>Rico Suter, mail@rsuter.com</author>
//-----------------------------------------------------------------------

using Windows.ApplicationModel;
using Windows.UI.Xaml;

namespace MyToolkit.UI
{
    internal class MyDependencyObject : DependencyObject
    {
    }

    public static class Designer
    {
        public static bool IsInDesignMode => DesignMode.DesignModeEnabled;
    }
}