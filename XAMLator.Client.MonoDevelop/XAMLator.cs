using System;
using System.Threading.Tasks;
using MonoDevelop.Ide;

namespace XAMLator.Client
{
	/// <summary>
	/// MonoDevelop addin implementation that monitors document changes
	/// and requests previews to the clients.
	/// </summary>
	public partial class XAMLatorMonitor
	{
		MonoDevelop.Ide.Gui.Document boundDoc;

		protected void MonitorEditorChanges()
		{
			IdeApp.Workbench.ActiveDocumentChanged += BindActiveDoc;
			BindActiveDoc(this, EventArgs.Empty);
		}

		void BindActiveDoc(object sender, EventArgs e)
		{
			var doc = IdeApp.Workbench.ActiveDocument;
			if (boundDoc == doc)
			{
				return;
			}
			if (boundDoc != null)
			{
				boundDoc.DocumentParsed -= BoundDoc_Saved;
				boundDoc = null;
			}

			if (doc?.FileName.Extension == ".xaml")
			{
				boundDoc = doc;
				Log.Information($"Monitoring XAML document {boundDoc.FileName}");
				boundDoc.Saved += BoundDoc_Saved;
				Preview();
			}
		}

		Task Preview()
		{
			Log.Information($"XAML document changed {boundDoc.Name}");
			return OnXAMLChanged(boundDoc.Editor.Text);
		}

		async void BoundDoc_Saved(object sender, EventArgs e)
		{
			await Preview();
		}
	}
}
