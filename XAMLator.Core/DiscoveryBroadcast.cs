using System;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace XAMLator
{
    /// <summary>
    /// Discovery broadcast message sent by devices to announce the service.
    /// </summary>
    public class DiscoveryBroadcast
    {
        /// <summary>
        /// Gets or sets the name of the device.
        /// </summary>
        /// <value>The name of the device.</value>
        public string DeviceName { get; set; }

        /// <summary>
        /// Gets or sets the device model.
        /// </summary>
        /// <value>The device model.</value>
        public string DeviceModel { get; set; }

        /// <summary>
        /// Gets the identifier.
        /// </summary>
        /// <value>The identifier.</value>
        [Newtonsoft.Json.JsonIgnore]
        public string Id => $"{DeviceName} ({DeviceModel})";

        /// <summary>
        /// Gets or sets the addresses.
        /// </summary>
        /// <value>The addresses.</value>
        public DiscoveryBroadcastAddress[] Addresses { get; set; }

        public override bool Equals(object obj)
        {
            var o = obj as DiscoveryBroadcast;
            if (o == null) return false;
            if (DeviceName != o.DeviceName || DeviceModel != o.DeviceModel || Addresses.Length != o.Addresses.Length) return false;
            for (var i = 0; i < Addresses.Length; i++)
            {
                if (!Addresses[i].Equals(o.Addresses[i]))
                    return false;
            }
            return true;
        }

        public override int GetHashCode()
        {
            var s = 1;
            if (Addresses != null)
            {
                foreach (var a in Addresses)
                {
                    s += a.GetHashCode();
                }
            }
            s += (DeviceName?.GetHashCode() + DeviceModel?.GetHashCode()) ?? 0;
            return s;
        }

        public static DiscoveryBroadcast CreateForDevice(int port)
        {
            var allInterfaces = NetworkInterface.GetAllNetworkInterfaces();
            var goodInterfaces =
                allInterfaces.Where(x => x.NetworkInterfaceType != NetworkInterfaceType.Loopback &&
                                    !x.Name.StartsWith("pdp_ip", StringComparison.Ordinal) &&
                                    x.OperationalStatus == OperationalStatus.Up);
            var iips = goodInterfaces.SelectMany(x =>
                x.GetIPProperties().UnicastAddresses
                .Where(y => y.Address.AddressFamily == AddressFamily.InterNetwork)
                .Select(y => new DiscoveryBroadcastAddress
                {
                    Address = y.Address.ToString(),
                    Port = port,
                    Interface = x.Name
                }));
            var r = new DiscoveryBroadcast
            {
                DeviceName = "Device",
                DeviceModel = "Model",
                Addresses = iips.ToArray()
            };
#if __IOS__
            var dev = UIKit.UIDevice.CurrentDevice;
            r.DeviceName = dev.Name;
            r.DeviceModel = dev.Model;
#endif
            return r;
        }
    }
}
