using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MonoDevelop.Ide;
using XAMLator.HttpServer;

namespace XAMLator.Client
{
	/// <summary>
	/// Listen for <see cref="DiscoveryBroadcast"/> messages published by devices
	/// announcing the XAMLator service.
	/// </summary>
	public class DiscoveryReceiver : RequestProcessor
	{
		public event EventHandler DevicesChanged;

		readonly Dictionary<DeviceInfo, DiscoveryBroadcast> devices;
		UdpClient listener;
		Thread thread;
		bool running;
		HttpHost host;
		int port;

		public DiscoveryReceiver()
		{
			devices = new Dictionary<DeviceInfo, DiscoveryBroadcast>();
			Post["/register"] = HandleRegisterDevice;
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

		public async Task<bool> Start()
		{
			host = await HttpHost.StartServer(this, Constants.DEFAULT_CLIENT_PORT, 1);
			if (host == null)
			{
				MessageService.ShowError($"XAMLator: Failed to start the server. There seems to be another instance of VS4MAC is running!");
				return false;
			}
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
			return true;
		}

		public void Stop()
		{
			running = false;
			host.StopListening();
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
			var json = Encoding.UTF8.GetString(bytes);
			AddDevice(Serializer.DeserializeJson<DiscoveryBroadcast>(json));
		}

		void AddDevice(DiscoveryBroadcast newBroadcast)
		{
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

		async Task<HttpResponse> HandleRegisterDevice(HttpRequest request)
		{
			JsonHttpResponse response = new JsonHttpResponse();

			StreamReader sr = new StreamReader(request.Body, Encoding.UTF8);
			string json = await sr.ReadToEndAsync();
			AddDevice(Serializer.DeserializeJson<DiscoveryBroadcast>(json));
			return response;
		}
	}
}
