//-----------------------------------------------------------------------
// <copyright file="Device.cs" company="MyToolkit">
//     Copyright (c) Rico Suter. All rights reserved.
// </copyright>
// <license>https://github.com/MyToolkit/MyToolkit/blob/master/LICENSE.md</license>
// <author>Rico Suter, mail@rsuter.com</author>
//-----------------------------------------------------------------------

using System;
using System.Linq;
using Windows.Foundation.Metadata;
using Windows.Networking.Connectivity;

namespace MyToolkit.Environment
{
    public class Device
    {
        private static string _deviceId;

        static Device()
        {
            HardwareButtonsType = Type.GetType(
                "Windows.Phone.UI.Input.HardwareButtons, " +
                "Windows, Version=255.255.255.255, Culture=neutral, " +
                "PublicKeyToken=null, ContentType=WindowsRuntime");
        }

        /// <summary>
        ///     Gets a unique ID which can be used to identify the current device.
        /// </summary>
        public static string DeviceId
        {
            get
            {
                return _deviceId ?? (_deviceId = NetworkInformation.GetConnectionProfiles()
                           .Where(p => p.GetNetworkConnectivityLevel() !=
                                       NetworkConnectivityLevel.ConstrainedInternetAccess)
                           .Select(p => p.NetworkAdapter.NetworkAdapterId).OrderBy(p => p).First().ToString());
            }
        }

        internal static Type HardwareButtonsType { get; }

        /// <summary>Gets a value indicating whether the current device has a hardware back key. </summary>
        public static bool HasHardwareBackKey => ApiInformation.IsTypePresent("Windows.Phone.UI.Input.HardwareButtons");
    }

    [Obsolete("Use Device class instead. 5/17/2014")]
    public class Machine : Device
    {
    }
}