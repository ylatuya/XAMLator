using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace XAMLator.Server
{
	/// <summary>
	/// Preview server that process HTTP requests, evaluates them in the <see cref="VM"/>
	/// and preview them with the <see cref="Previewer"/>.
	/// </summary>
	public class PreviewServer
	{
		static readonly PreviewServer serverInstance = new PreviewServer();

		VM vm;
		TaskScheduler mainScheduler;
		IPreviewer previewer;
		bool isRunning;
		ITcpCommunicatorClient client;
		ErrorViewModel errorViewModel;
		IUIToolkit uiToolkit;

		internal static PreviewServer Instance => serverInstance;

		internal PreviewServer()
		{
			errorViewModel = new ErrorViewModel();
		}

		public static Task<bool> Run(Dictionary<Type, object> viewModelsMapping = null,
			IPreviewer previewer = null, string ideIP = null, int idePort = Constants.DEFAULT_PORT)
		{
			return Instance.RunInternal(viewModelsMapping, previewer, ideIP, idePort,
				new UIToolkit(), new TcpCommunicatorClient());
		}

		internal async Task<bool> RunInternal(Dictionary<Type, object> viewModelsMapping,
			IPreviewer previewer, string ideIP, int idePort, IUIToolkit uiToolkit, ITcpCommunicatorClient client)
		{
			if (isRunning)
			{
				return true;
			}

			this.uiToolkit = uiToolkit;

			if (client == null)
			{
				client = new TcpCommunicatorClient();
			}
			this.client = client;
			client.DataReceived += HandleDataReceived;

			mainScheduler = TaskScheduler.FromCurrentSynchronizationContext();
			await RegisterDevice(ideIP, idePort);
			if (viewModelsMapping == null)
			{
				viewModelsMapping = new Dictionary<Type, object>();
			}
			if (previewer == null)
			{
				previewer = new Previewer(viewModelsMapping);
			}
			this.previewer = previewer;
			vm = new VM();
			isRunning = true;
			return true;
		}

		async Task RegisterDevice(string ideIP, int idePort)
		{
			ideIP = string.IsNullOrEmpty(ideIP) ? GetIdeIPFromResource() : ideIP;
			try
			{
				Log.Information($"Connecting to IDE at tcp://{ideIP}:{idePort}");
				await client.Connect(ideIP, idePort);
			}
			catch (Exception ex)
			{
				Log.Error($"Couldn't register device at {ideIP}");
				Log.Exception(ex);
			}
		}

		string GetIdeIPFromResource()
		{
			try
			{
				using (Stream stream = GetType().Assembly.GetManifestResourceStream(Constants.IDE_IP_RESOURCE_NAME))
				using (StreamReader reader = new StreamReader(stream))
				{
					return reader.ReadToEnd().Split('\n')[0].Trim();
				}
			}
			catch (Exception ex)
			{
				Log.Exception(ex);
				return null;
			}
		}

		async void HandleDataReceived(object sender, object e)
		{
			var container = e as JContainer;
			string type = (string)container["Type"];

			if (type == typeof(EvalRequestMessage).Name)
			{
				await HandleEvalRequest(container.ToObject<EvalRequestMessage>());
			}
			else if (type == typeof(ErrorMessage).Name)
			{
				var errorMessage = container.ToObject<ErrorMessage>();
				await uiToolkit.RunInUIThreadAsync(async () =>
				{
					errorViewModel.SetError("Oh no! An exception!", errorMessage.Exception);
					await previewer.NotifyError(errorViewModel);
				});
			}
		}

		async Task HandleEvalRequest(EvalRequestMessage request)
		{
			EvalResponse evalResponse = new EvalResponse();
			EvalResult result;
			try
			{
				result = await vm.Eval(request, mainScheduler, CancellationToken.None);
				if (result.HasResult || result.ResultType != null)
				{
					await uiToolkit.RunInUIThreadAsync(async () =>
					   {
						   try
						   {
							   await previewer.Preview(result);
						   }
						   catch (Exception ex)
						   {
							   errorViewModel.SetError("Oh no! An exception!", ex);
							   await previewer.NotifyError(errorViewModel);
						   }
					   });
				}
				else
				{
					await uiToolkit.RunInUIThreadAsync(async () =>
					{
						errorViewModel.SetError("Oh no! An evaluation error!", result);
						await previewer.NotifyError(errorViewModel);
					});
				}
			}
			catch (Exception ex)
			{
				Log.Exception(ex);
			}
		}
	}
}
