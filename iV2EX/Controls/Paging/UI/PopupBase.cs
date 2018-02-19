//-----------------------------------------------------------------------
// <copyright file="PopupBase.cs" company="MyToolkit">
//     Copyright (c) Rico Suter. All rights reserved.
// </copyright>
// <license>https://github.com/MyToolkit/MyToolkit/blob/master/LICENSE.md</license>
// <author>Rico Suter, mail@rsuter.com</author>
//-----------------------------------------------------------------------

using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;

namespace MyToolkit.UI
{
    public class PopupBase : UserControl
    {
        public void Close()
        {
            ((Popup) Parent).IsOpen = false;
        }
    }
}