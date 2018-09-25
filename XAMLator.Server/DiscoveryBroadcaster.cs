using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Net.Http;
using System.Timers;
using System.Threading.Tasks;
using System.Text;

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
		readonly bool localDevicesOnly;

		UdpClient client;
		Timer timer = new Timer(3000);

		public DiscoveryBroadcaster(int httpPort = 0, bool localDevicesOnly = false)
		{
			this.localDevicesOnly = localDevicesOnly;
			this.httpPort = (httpPort == 0) ? Constants.DEFAULT_PORT : httpPort;
		}

		public async Task Start()
		{
			if (localDevicesOnly)
			{
				var httpClient = new HttpClient();
				var broadcast = DiscoveryBroadcast.CreateForDevice(httpPort);
				foreach (var address in broadcast.Addresses)
				{
					var content = new StringContent(Serializer.SerializeJson(broadcast),
													Encoding.UTF8, "application/json");
					try
					{
						var url = $"http://{address.Address}:{Constants.DEFAULT_CLIENT_PORT}/register";
						Log.Debug($"Registering device at {url}");
						var res = await httpClient.PostAsync(url, content);
						if (res.IsSuccessStatusCode)
						{
							break;
						}
					}
					catch (Exception ex)
					{
						Log.Exception(ex);
					}
				}
			}
			else
			{
				StartBroadcast();
			}
		}

		void StartBroadcast()
		{
			client = new UdpClient()
			{
				EnableBroadcast = true
			};
			timer.Elapsed += Timer_Elapsed;
			timer.Start();

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
