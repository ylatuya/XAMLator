namespace XAMLator
{
    /// <summary>
    /// Represents Discovery broadcast address.
    /// </summary>
    public class DiscoveryBroadcastAddress
    {
        /// <summary>
        /// Gets or sets the IP address.
        /// </summary>
        /// <value>The address.</value>
        public string Address { get; set; }

        /// <summary>
        /// Gets or sets the port where the service is running.
        /// </summary>
        /// <value>The port.</value>
        public int Port { get; set; }

        /// <summary>
        /// Gets or sets the name of the interface.
        /// </summary>
        /// <value>The interface name.</value>
        public string Interface { get; set; }

        public override bool Equals(object obj)
        {
            var o = obj as DiscoveryBroadcastAddress;
            if (o == null) return false;
            if (Address != o.Address || Interface != o.Interface || Port != o.Port) return false;
            return true;
        }

        public override int GetHashCode()
        {
            var s = 1;
            s += Address.GetHashCode() + Interface.GetHashCode() + Port.GetHashCode();
            return s;
        }
    }
}
