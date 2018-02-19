//-----------------------------------------------------------------------
// <copyright file="BackKeyPressedHandler.cs" company="MyToolkit">
//     Copyright (c) Rico Suter. All rights reserved.
// </copyright>
// <license>https://github.com/MyToolkit/MyToolkit/blob/master/LICENSE.md</license>
// <author>Rico Suter, mail@rsuter.com</author>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Core;

namespace MyToolkit.Paging.Handlers
{
    /// <summary>
    ///     Registers for the hardware back key button on Windows Phone and calls the registered methods when the event
    ///     occurs.
    /// </summary>
    public class BackKeyPressedHandler
    {
        private readonly List<Tuple<MtPage, Func<object, bool>>> _handlers;

        //private Type _hardwareButtonsType = null;
        //private object _registrationToken;
        private bool _isEventRegistered;

        public BackKeyPressedHandler()
        {
            _handlers = new List<Tuple<MtPage, Func<object, bool>>>();
        }

        /// <summary>Adds a back key handler for a given page. </summary>
        /// <param name="page">The page. </param>
        /// <param name="handler">The handler. </param>
        public void Add(MtPage page, Func<object, bool> handler)
        {
            if (!_isEventRegistered)
            {
                SystemNavigationManager.GetForCurrentView().BackRequested += OnBackKeyPressed;
                _isEventRegistered = true;
            }

            _handlers.Insert(0, new Tuple<MtPage, Func<object, bool>>(page, handler));
        }

        /// <summary>Removes a back key pressed handler for a given page. </summary>
        /// <param name="page">The page. </param>
        public void Remove(MtPage page)
        {
            _handlers.Remove(_handlers.Single(h => h.Item1 == page));

            if (_handlers.Count == 0)
            {
                SystemNavigationManager.GetForCurrentView().BackRequested -= OnBackKeyPressed;
                _isEventRegistered = false;
            }
        }

        private void OnBackKeyPressed(object sender, BackRequestedEventArgs args)
        {
            var handled = args.Handled;
            if (handled)
                return;

            foreach (var item in _handlers)
            {
                handled = item.Item2(sender);
                args.Handled = handled;
                if (handled)
                    return;
            }
        }
    }
}