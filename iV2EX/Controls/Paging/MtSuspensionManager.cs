//-----------------------------------------------------------------------
// <copyright file="MtSuspensionManager.cs" company="MyToolkit">
//     Copyright (c) Rico Suter. All rights reserved.
// </copyright>
// <license>https://github.com/MyToolkit/MyToolkit/blob/master/LICENSE.md</license>
// <author>Rico Suter, mail@rsuter.com</author>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml;

namespace MyToolkit.Paging
{
    public delegate void SessionStateRestoredEventHandler(object sender, Dictionary<string, object> e);

    /// <summary>Stores and loads global session state for application life cycle management. </summary>
    public static class MtSuspensionManager
    {
        private const string SessionStateFilename = "_sessionState.xml";

        private static readonly List<WeakReference<MtFrame>> RegisteredFrames = new List<WeakReference<MtFrame>>();

        private static readonly DependencyProperty FrameSessionStateKeyProperty =
            DependencyProperty.RegisterAttached("_FrameSessionStateKey", typeof(string), typeof(MtSuspensionManager),
                null);

        private static readonly DependencyProperty FrameSessionStateProperty =
            DependencyProperty.RegisterAttached("_FrameSessionState", typeof(Dictionary<string, object>),
                typeof(MtSuspensionManager), null);

        /// <summary>
        ///     Gets the session state for the current session.
        ///     The objects must be serializable with the <see cref="DataContractSerializer" />
        ///     and the types provided in <see cref="KnownTypes" />.
        /// </summary>
        public static Dictionary<string, object> SessionState { get; private set; } = new Dictionary<string, object>();

        /// <summary>
        ///     Gets a list of known types for the <see cref="DataContractSerializer" />
        ///     to serialize the <see cref="SessionState" />.
        /// </summary>
        public static HashSet<Type> KnownTypes { get; } = new HashSet<Type>();

        /// <summary>Occurs when the session state has been restored.</summary>
        public static event SessionStateRestoredEventHandler SessionStateRestored;

        /// <summary>Saves the current session state. </summary>
        public static async Task SaveAsync()
        {
            foreach (var weakFrameReference in RegisteredFrames)
                if (weakFrameReference.TryGetTarget(out var frame))
                    SaveFrameNavigationState(frame);

            var sessionData = new MemoryStream();
            var serializer = new DataContractSerializer(typeof(Dictionary<string, object>), KnownTypes);
            serializer.WriteObject(sessionData, SessionState);

            var folder = ApplicationData.Current.LocalFolder;
            var file = await folder.CreateFileAsync(SessionStateFilename, CreationCollisionOption.ReplaceExisting);
            await FileIO.WriteBufferAsync(file, sessionData.GetWindowsRuntimeBuffer());
        }

        /// <summary>Restores the previously stored session state. </summary>
        public static async Task RestoreAsync()
        {
            var folder = ApplicationData.Current.LocalFolder;
            using (var stream = await folder.OpenStreamForReadAsync(SessionStateFilename))
            {
                var serializer = new DataContractSerializer(typeof(Dictionary<string, object>), KnownTypes);
                SessionState = (Dictionary<string, object>) serializer.ReadObject(stream);

                SessionStateRestored?.Invoke(null, SessionState);
            }

            foreach (var weakFrameReference in RegisteredFrames)
                if (weakFrameReference.TryGetTarget(out var frame))
                {
                    frame.ClearValue(FrameSessionStateProperty);
                    RestoreFrameNavigationState(frame);
                }
        }

        /// <summary>Registers a frame so that its navigation state can be saved and restored. </summary>
        /// <param name="frame">The frame. </param>
        /// <param name="sessionStateKey">The session state key. </param>
        public static void RegisterFrame(MtFrame frame, string sessionStateKey)
        {
            if (frame.GetValue(FrameSessionStateKeyProperty) != null)
                throw new InvalidOperationException("Frames can only be registered to one session state key");

            if (frame.GetValue(FrameSessionStateProperty) != null)
                throw new InvalidOperationException(
                    "Frames must be either be registered before accessing frame session state, or not registered at all");

            frame.SetValue(FrameSessionStateKeyProperty, sessionStateKey);
            RegisteredFrames.Add(new WeakReference<MtFrame>(frame));

            RestoreFrameNavigationState(frame);
        }

        /// <summary>Deregisters a frame. </summary>
        /// <param name="frame">The frame. </param>
        public static void DeregisterFrame(MtFrame frame)
        {
            SessionState.Remove((string) frame.GetValue(FrameSessionStateKeyProperty));
            RegisteredFrames.RemoveAll(
                weakFrameReference => !weakFrameReference.TryGetTarget(out var testFrame) || testFrame == frame);
        }

        /// <summary>Gets the session state for a given frame. </summary>
        /// <param name="frame">The frame. </param>
        /// <returns>The session state. </returns>
        public static Dictionary<string, object> SessionStateForFrame(MtFrame frame)
        {
            var frameState = (Dictionary<string, object>) frame.GetValue(FrameSessionStateProperty);
            if (frameState == null)
            {
                var frameSessionKey = (string) frame.GetValue(FrameSessionStateKeyProperty);
                if (frameSessionKey != null)
                {
                    if (!SessionState.ContainsKey(frameSessionKey))
                        SessionState[frameSessionKey] = new Dictionary<string, object>();
                    frameState = (Dictionary<string, object>) SessionState[frameSessionKey];
                }
                else
                {
                    frameState = new Dictionary<string, object>();
                }

                frame.SetValue(FrameSessionStateProperty, frameState);
            }

            return frameState;
        }

        private static void RestoreFrameNavigationState(MtFrame frame)
        {
            var frameState = SessionStateForFrame(frame);
            if (frameState.ContainsKey("Navigation"))
                frame.SetNavigationState((string) frameState["Navigation"]);
        }

        private static void SaveFrameNavigationState(MtFrame frame)
        {
            var frameState = SessionStateForFrame(frame);
            frameState["Navigation"] = frame.GetNavigationState();
        }
    }
}