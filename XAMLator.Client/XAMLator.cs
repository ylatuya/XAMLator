using System;
using Newtonsoft.Json.Linq;

namespace XAMLator.Client
{
	public class XAMLatorMonitor
	{
		ITcpCommunicatorServer server;

		public static XAMLatorMonitor Init(IIDE ide)
		{
			Instance = new XAMLatorMonitor(ide);
			return Instance;
		}

		public static XAMLatorMonitor Instance { get; private set; }

		internal XAMLatorMonitor(IIDE ide, ITcpCommunicatorServer server = null)
		{
			IDE = ide;
			if (server == null)
			{
				server = new TcpCommunicatorServer();
			}
			this.server = server;
			server.DataReceived += HandleDataReceived;
			ide.DocumentChanged += HandleDocumentChanged;
		}

		public IIDE IDE { get; private set; }

		public void StartMonitoring()
		{
			StartMonitoring(Constants.DEFAULT_PORT);
		}

		internal void StartMonitoring(int port)
		{
			IDE.MonitorEditorChanges();
			server.StartListening(port);
		}

		async void HandleDocumentChanged(object sender, DocumentChangedEventArgs e)
		{
			if (server.ClientsCount == 0)
			{
				return;
			}
			FormsViewClassDeclaration classDecl = null;

			try
			{
				classDecl = await DocumentParser.ParseDocument(e.Filename, e.Text, e.SyntaxTree, e.SemanticModel);
			}
			catch (Exception ex)
			{
				Log.Exception(ex);
				await server.Send(new ErrorMessage($"Error parsing {e.Filename}",
					ex.ToString()));
				return;
			}

			if (classDecl == null)
			{
				return;
			}

			EvalRequestMessage request = new EvalRequestMessage
			{
				Declarations = classDecl.Code,
				NeedsRebuild = classDecl.NeedsRebuild,
				OriginalTypeName = classDecl.FullNamespace,
				NewTypeName = classDecl.CurrentFullNamespace,
				Xaml = classDecl.Xaml,
				XamlResourceName = classDecl.XamlResourceId,
				StyleSheets = classDecl.StyleSheets
			};
			try
			{
				await server.Send(request);
			}
			catch (Exception ex)
			{
				Log.Exception(ex);
			}
		}

		void HandleDataReceived(object sender, object e)
		{
			var container = e as JContainer;
			string type = (string)container["Type"];

			if (type == typeof(ResetMessage).Name)
			{
				FormsViewClassDeclaration.Reset();
			}
		}
	}
}