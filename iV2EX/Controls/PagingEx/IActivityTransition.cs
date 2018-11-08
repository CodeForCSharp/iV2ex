using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace PagingEx
{
    /// <summary>Enumeration of the possible activity insertion modes. </summary>
    public enum ActivityInsertionMode
    {
        /// <summary>Inserts the new activity over the previous activity before starting the animations so that both activities are in the visual tree during the animations. </summary>
        NewAbove,

        /// <summary>Inserts the new activity below the previous activity before starting the animations so that both activities are in the visual tree during the animations. </summary>
        NewBelow,
    }
    public interface IActivityTransition
    {
        ActivityInsertionMode InsertionMode { get; }

        Task OnStart(FrameworkElement newActivity, FrameworkElement currentActivity);

        Task OnClose(FrameworkElement closeActivity, FrameworkElement previousActivity);
    }

}
