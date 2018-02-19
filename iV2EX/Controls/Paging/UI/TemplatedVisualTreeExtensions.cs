﻿//-----------------------------------------------------------------------
// <copyright file="TemplatedVisualTreeExtensions.cs" company="MyToolkit">
//     Copyright (c) Rico Suter. All rights reserved.
// </copyright>
// <license>https://github.com/MyToolkit/MyToolkit/blob/master/LICENSE.md</license>
// <author>Rico Suter, mail@rsuter.com</author>
//-----------------------------------------------------------------------

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace MyToolkit.UI
{
    /// <summary>
    ///     A static class providing methods for working with the visual tree using generics.
    /// </summary>
    public static class TemplatedVisualTreeExtensions
    {
        /// <summary>
        ///     Retrieves the first logical child of a specified type using a
        ///     breadth-first search.  A visual element is assumed to be a logical
        ///     child of another visual element if they are in the same namescope.
        ///     For performance reasons this method manually manages the queue
        ///     instead of using recursion.
        /// </summary>
        /// <param name="parent">The parent framework element.</param>
        /// <param name="applyTemplates">Specifies whether to apply templates on the traversed framework elements</param>
        /// <returns>The first logical child of the framework element of the specified type.</returns>
        internal static T GetFirstLogicalChildByType<T>(this FrameworkElement parent, bool applyTemplates)
            where T : FrameworkElement
        {
            Debug.Assert(parent != null, "The parent cannot be null.");

            var queue = new Queue<FrameworkElement>();
            queue.Enqueue(parent);

            while (queue.Count > 0)
            {
                var element = queue.Dequeue();
                var elementAsControl = element as Control;
                if (applyTemplates)
                    elementAsControl?.ApplyTemplate();

                if (element is T && element != parent)
                    return (T) element;

                foreach (var child in element.GetVisualChildren())
                {
                    var visualChild = child;
                    if (visualChild != null)
                        queue.Enqueue(visualChild);
                }
            }

            return null;
        }

        /// <summary>
        ///     Retrieves all the logical children of a specified type using a
        ///     breadth-first search.  A visual element is assumed to be a logical
        ///     child of another visual element if they are in the same namescope.
        ///     For performance reasons this method manually manages the queue
        ///     instead of using recursion.
        /// </summary>
        /// <param name="parent">The parent framework element.</param>
        /// <param name="applyTemplates">Specifies whether to apply templates on the traversed framework elements</param>
        /// <returns>The logical children of the framework element of the specified type.</returns>
        internal static IEnumerable<T> GetLogicalChildrenByType<T>(this FrameworkElement parent, bool applyTemplates)
            where T : FrameworkElement
        {
            Debug.Assert(parent != null, "The parent cannot be null.");

            if (applyTemplates)
                (parent as Control)?.ApplyTemplate();

            var queue =
                new Queue<FrameworkElement>(parent.GetVisualChildren().OfType<FrameworkElement>());

            while (queue.Count > 0)
            {
                var element = queue.Dequeue();
                if (applyTemplates)
                    (element as Control)?.ApplyTemplate();

                if (element is T)
                    yield return (T) element;

                foreach (var visualChild in element.GetVisualChildren().OfType<FrameworkElement>())
                    queue.Enqueue(visualChild);
            }
        }
    }
}