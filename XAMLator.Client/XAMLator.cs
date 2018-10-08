using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

namespace XAMLator.Client
{
	public class XAMLatorMonitor
	{
		static XAMLatorMonitor instance;

		readonly DiscoveryReceiver discovery;
		Dictionary<DeviceInfo, HttpClient> clients;
		object devicesLock = new object();
		IIDE ide;

		XAMLatorMonitor(IIDE ide)
		{
			this.ide = ide;
			clients = new Dictionary<DeviceInfo, HttpClient>();
			discovery = new DiscoveryReceiver();
			discovery.DevicesChanged += HandleDiscoveryDevicesChanged;
			discovery.Start();
			ide.DocumentChanged += HandleDocumentChanged;
		}

		public static XAMLatorMonitor Init(IIDE ide)
		{
			instance = new XAMLatorMonitor(ide);
			return instance;
		}

		public static XAMLatorMonitor Instance => instance;

		public IIDE IDE => ide;

		public void StartMonitoring()
		{
			ide.MonitorEditorChanges();
		}

		async void HandleDocumentChanged(object sender, DocumentChangedEventArgs e)
		{
			if (clients.Count == 0)
			{
				return;
			}

			var classDecl = await DocumentParser.ParseDocument(e.Filename, e.Text, e.SyntaxTree, e.SemanticModel);

			EvalRequest request = new EvalRequest
			{
				Declarations = classDecl?.Code,
				NeedsRebuild = classDecl.NeedsRebuild,
				NewTypeExpression = classDecl.NewInstanceExpression,
				Xaml = classDecl.Xaml
			};
			await clients.Values.ToList().ForEachAsync(10, async (client) =>
			{
				var res = await client.PreviewXaml(request);
			});
		}

		void HandleDiscoveryDevicesChanged(object sender, EventArgs e)
		{
			var currentDevices = discovery.Devices;
			lock (devicesLock)
			{
				// Remove disconnected devices
				foreach (var deviceName in clients.Keys)
				{
					if (!currentDevices.Contains(deviceName))
					{
						clients.Remove(deviceName);
					}
				}

				// Add new devices
				foreach (var device in currentDevices)
				{
					if (!clients.ContainsKey(device))
					{
						clients.Add(device, new HttpClient($"http://{device.IP}:{device.Port}"));
					}
				}
			}
		}
	}
}