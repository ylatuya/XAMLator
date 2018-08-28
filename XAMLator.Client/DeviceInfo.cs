using System;
using System.Collections.Generic;

namespace XAMLator.Client
{
    /// <summary>
    /// Device information where the service is running.
    /// </summary>
    public class DeviceInfo : IEquatable<DeviceInfo>
    {
        public DeviceInfo(string id, string ip, int port)
        {
            ID = id;
            IP = ip;
            Port = port;
        }

        /// <summary>
        /// Gets the device identifier.
        /// </summary>
        /// <value>The identifier.</value>
        public string ID { get; }

        /// <summary>
        /// Gets the IP of the device where the service is running.
        /// </summary>
        /// <value>The IP.</value>
        public string IP { get; }

        /// <summary>
        /// Gets the port where the service is running.
        /// </summary>
        /// <value>The port.</value>
        public int Port { get; }

        public bool Equals(DeviceInfo other)
        {
            return other != null &&
                   ID == other.ID &&
                   IP == other.IP &&
                   Port == other.Port;
        }

        public override int GetHashCode()
        {
            var hashCode = -305899678;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(ID);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(IP);
            hashCode = hashCode * -1521134295 + EqualityComparer<int>.Default.GetHashCode(Port);
            return hashCode;
        }

        public static bool operator ==(DeviceInfo info1, DeviceInfo info2)
        {
            return EqualityComparer<DeviceInfo>.Default.Equals(info1, info2);
        }

        public static bool operator !=(DeviceInfo info1, DeviceInfo info2)
        {
            return !(info1 == info2);
        }
    }
}
