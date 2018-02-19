//-----------------------------------------------------------------------
// <copyright file="Keyboard.cs" company="MyToolkit">
//     Copyright (c) Rico Suter. All rights reserved.
// </copyright>
// <license>https://github.com/MyToolkit/MyToolkit/blob/master/LICENSE.md</license>
// <author>Rico Suter, mail@rsuter.com</author>
//-----------------------------------------------------------------------

using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml;

namespace MyToolkit.Input
{
    public static class Keyboard
    {
        public static bool IsControlKeyDown => IsKeyDown(VirtualKey.Control);

        public static bool IsShiftKeyDown => IsKeyDown(VirtualKey.Shift);

        public static bool IsAltKeyDown => IsKeyDown(VirtualKey.LeftMenu);

        public static bool IsKeyDown(VirtualKey key)
        {
            return (Window.Current.CoreWindow.GetKeyState(key) & CoreVirtualKeyStates.Down) ==
                   CoreVirtualKeyStates.Down;
        }
    }
}