using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Timers;

namespace XAMLator.Server
{
    /// <summary>
    /// Publishes <see cref="DiscoveryBroadcast"/> messages announcing the service
    /// running in the device.
    /// </summary>
    public class DiscoveryBroadcaster
    {
        readonly IPEndPoint broadcastEndpoint = new IPEndPoint(IPAddress.Broadcast, Constants.DISCOVERY_BROADCAST_RECEIVER_PORT);
        readonly int httpPort;

        UdpClient client;
        Timer timer = new Timer(3000);

        public DiscoveryBroadcaster(int httpPort = 0)
        {
            client = new UdpClient()
            {
                EnableBroadcast = true
            };
            timer.Elapsed += Timer_Elapsed;
            timer.Start();
            this.httpPort = (httpPort == 0) ? Constants.DEFAULT_PORT : httpPort;
        }

        void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                var broadcast = DiscoveryBroadcast.CreateForDevice(httpPort);
                var json = Serializer.SerializeJson(broadcast);
                var bytes = System.Text.Encoding.UTF8.GetBytes(json);

                client.Send(bytes, bytes.Length, broadcastEndpoint);
                //Debug.WriteLine ($"BROADCAST {json}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"FAILED TO BROADCAST ON PORT {Constants.DISCOVERY_BROADCAST_PORT}: {ex}");
            }
        }
    }
}
