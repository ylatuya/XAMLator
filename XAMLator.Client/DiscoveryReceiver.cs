using System;
using System.Diagnostics;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using MonoDevelop.Ide;

namespace XAMLator.Client
{
    /// <summary>
    /// Listen for <see cref="DiscoveryBroadcast"/> messages published by devices
    /// announcing the XAMLator service.
    /// </summary>
    public class DiscoveryReceiver
    {
        public event EventHandler DevicesChanged;

        readonly UdpClient listener;
        readonly Dictionary<DeviceInfo, DiscoveryBroadcast> devices;
        Thread thread;
        bool running;


        public DiscoveryReceiver()
        {
            devices = new Dictionary<DeviceInfo, DiscoveryBroadcast>();
            try
            {
                listener = new UdpClient(Constants.DISCOVERY_BROADCAST_RECEIVER_PORT, AddressFamily.InterNetwork);
            }
            catch (Exception ex)
            {
                MessageService.ShowError($"XAMLator: Failed to start the discovery receiver", ex);
                Debug.WriteLine("XAMLator: Failed to listen: " + ex);
                listener = null;
            }
            if (listener != null)
            {
                thread = new Thread(Run);
                thread.Start();
            }
            running = true;
        }

        /// <summary>
        /// Gets the current list of devices running announcing the service.
        /// </summary>
        public DeviceInfo[] Devices
        {
            get
            {
                lock (devices)
                {
                    return devices.Keys.ToArray();
                }
            }
        }

        public void Stop()
        {
            running = false;
            thread = null;
        }

        void Run()
        {
            while (running)
            {
                try
                {
                    Listen();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("DISCOVERY RECEIVE FAILED " + ex);
                }
            }
        }

        void Listen()
        {
            var broadcastEndpoint = new IPEndPoint(IPAddress.Any, Constants.DISCOVERY_BROADCAST_RECEIVER_PORT);

            var bytes = listener.Receive(ref broadcastEndpoint);

            var json = System.Text.Encoding.UTF8.GetString(bytes);

            var newBroadcast = Newtonsoft.Json.JsonConvert.DeserializeObject<DiscoveryBroadcast>(json);

            bool changed = false;
            lock (devices)
            {
                if (newBroadcast.Addresses.Length == 0)
                {
                    return;
                }
                DeviceInfo device = new DeviceInfo(newBroadcast.Id, newBroadcast.Addresses[0].Address, newBroadcast.Addresses[0].Port);
                DiscoveryBroadcast oldBroadcast;
                if (devices.TryGetValue(device, out oldBroadcast))
                {
                    if (!oldBroadcast.Equals(newBroadcast))
                    {
                        changed = true;
                        devices[device] = newBroadcast;
                    }
                }
                else
                {
                    changed = true;
                    devices[device] = newBroadcast;
                }
            }
            if (changed)
            {
                DevicesChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}