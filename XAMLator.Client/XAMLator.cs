using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

namespace XAMLator.Client
{
	public partial class XAMLatorMonitor
	{
		static readonly XAMLatorMonitor instance = new XAMLatorMonitor();

		readonly DiscoveryReceiver discovery;
		Dictionary<DeviceInfo, HttpClient> clients;
		object devicesLock = new object();

		XAMLatorMonitor()
		{
			clients = new Dictionary<DeviceInfo, HttpClient>();
			discovery = new DiscoveryReceiver();
			discovery.DevicesChanged += HandleDiscoveryDevicesChanged;
			discovery.Start();
		}

		public static XAMLatorMonitor Instance
		{
			get
			{
				return instance;
			}
		}

		public void StartMonitoring()
		{
			MonitorEditorChanges();
		}

		async Task OnDocumentChanged(string fileName, string text, SyntaxTree syntaxTree, SemanticModel semanticModel)
		{
			if (clients.Count == 0)
			{
				return;
			}
			var classDecl = DocumentParser.ParseDocument(fileName, text, syntaxTree, semanticModel);

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