//-----------------------------------------------------------------------
// <copyright file="MtNotifyCollectionChangedEventArgs.cs" company="MyToolkit">
//     Copyright (c) Rico Suter. All rights reserved.
// </copyright>
// <license>https://github.com/MyToolkit/MyToolkit/blob/master/LICENSE.md</license>
// <author>Rico Suter, mail@rsuter.com</author>
//-----------------------------------------------------------------------

using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

namespace MyToolkit.Collections
{
    public class MtNotifyCollectionChangedEventArgs<T> : PropertyChangedEventArgs,
        IExtendedNotifyCollectionChangedEventArgs
    {
        public MtNotifyCollectionChangedEventArgs(IReadOnlyList<T> addedItems, IReadOnlyList<T> removedItems,
            IReadOnlyList<T> oldCollection)
            : base(null)
        {
            AddedItems = addedItems;
            RemovedItems = removedItems;
            OldCollection = oldCollection;
        }

        /// <summary>Gets or sets the list of added items. </summary>
        public IReadOnlyList<T> AddedItems { get; }

        /// <summary>Gets or sets the list of removed items. </summary>
        public IReadOnlyList<T> RemovedItems { get; }

        /// <summary>
        ///     Gets the previous collection (only provided when enabled in the <see cref="MtObservableCollection{T}" />
        ///     object).
        /// </summary>
        public IReadOnlyList<T> OldCollection { get; }

        IEnumerable IExtendedNotifyCollectionChangedEventArgs.RemovedItems => RemovedItems;

        IEnumerable IExtendedNotifyCollectionChangedEventArgs.AddedItems => AddedItems;

        IEnumerable IExtendedNotifyCollectionChangedEventArgs.OldCollection => OldCollection;
    }

    public interface IExtendedNotifyCollectionChangedEventArgs
    {
        /// <summary>Gets the list of added items. </summary>
        IEnumerable AddedItems { get; }

        /// <summary>Gets the list of removed items. </summary>
        IEnumerable RemovedItems { get; }

        /// <summary>
        ///     Gets the previous collection (only provided when enabled in the <see cref="MtObservableCollection{T}" />
        ///     object).
        /// </summary>
        IEnumerable OldCollection { get; }
    }
}