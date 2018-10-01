using System;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using MonoDevelop.Core;
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
				boundDoc.Saved -= HandleDocumentSaved;
				boundDoc.DocumentParsed -= HandleDocumentParsed;
				boundDoc = null;
			}

			var ext = doc?.FileName.Extension;
			if (ext == ".xaml" || ext == ".cs")
			{
				boundDoc = doc;
				Log.Information($"Monitoring document {boundDoc.FileName}");
				if (ext == ".xaml")
				{
					boundDoc.Saved += HandleDocumentSaved;
				}
				else
				{
					boundDoc.DocumentParsed += HandleDocumentParsed;
				}
				Preview();
			}
		}

		Task Preview(SyntaxTree syntaxTree = null, SemanticModel semanticModel = null)
		{
			Log.Information($"XAML document changed {boundDoc.Name}");
			return OnDocumentChanged(boundDoc.FileName, boundDoc.Editor.Text, syntaxTree, semanticModel);
		}

		async void HandleDocumentParsed(object sender, EventArgs e)
		{
			await Preview(
				await boundDoc.AnalysisDocument.GetSyntaxTreeAsync(),
				await boundDoc.AnalysisDocument.GetSemanticModelAsync()
			);
		}

		async void HandleDocumentSaved(object sender, EventArgs e)
		{
			await Preview();
		}
	}
}
