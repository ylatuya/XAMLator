using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

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

        protected virtual async Task OnXAMLChanged(string xaml)
        {
            if (clients.Count == 0)
            {
                return;
            }

            var doc = ParseXAML(xaml);
            if (doc == null)
            {
                return;
            }

            List<HttpClient> currentClients = clients.Values.ToList();

            await currentClients.ForEachAsync(10, async (client) =>
            {
                var res = await client.PreviewXaml(doc);
            });
        }

        XAMLDocument ParseXAML(string xaml)
        {
            try
            {
                using (var stream = new StringReader(xaml))
                {
                    var reader = XmlReader.Create(stream);
                    var xdoc = XDocument.Load(reader);
                    XNamespace x = "http://schemas.microsoft.com/winfx/2009/xaml";
                    var classAttribute = xdoc.Root.Attribute(x + "Class");
                    CleanAutomationIds(xdoc.Root);
                    xaml = xdoc.ToString();
                    return new XAMLDocument(xaml, classAttribute.Value);
                }
            }
            catch (Exception ex)
            {
                Log.Exception(ex);
                return null;
            }
        }

        void CleanAutomationIds(XElement xdoc)
        {
            xdoc.SetAttributeValue("AutomationId", null);
            foreach (var el in xdoc.Elements())
            {
                CleanAutomationIds(el);
            }
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
