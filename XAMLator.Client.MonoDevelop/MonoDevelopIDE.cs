using System;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using MonoDevelop.Ide;
using Document = MonoDevelop.Ide.Gui.Document;

namespace XAMLator.Client
{
	/// <summary>
	/// MonoDevelop implementation of <see cref="IIDE"/>.
	/// </summary>
	public class MonoDevelopIDE : IIDE
	{
		public event EventHandler<DocumentChangedEventArgs> DocumentChanged;

		Document boundDoc;

		public void MonitorEditorChanges()
		{
			IdeApp.Workbench.ActiveDocumentChanged += BindActiveDoc;
			BindActiveDoc(this, EventArgs.Empty);
		}

		public void ShowError(string error, Exception ex)
		{
			MessageService.ShowError(error, ex);
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
				boundDoc.AnalysisDocumentChanged -= HandleAnalysisDocumentChanged;
				boundDoc = null;
			}

			var ext = doc?.FileName.Extension;
			if (ext == ".xaml" || ext == ".cs")
			{
				boundDoc = doc;
				Log.Information($"Monitoring document {boundDoc.FileName}");
				boundDoc.Saved += HandleDocumentSaved;
				boundDoc.AnalysisDocumentChanged += HandleAnalysisDocumentChanged;
				HandleDocumentSaved(boundDoc, new EventArgs());
			}
		}

		void HandleAnalysisDocumentChanged(object sender, EventArgs e)
		{
			// This event is sent only once when the document is parsed the first
			// and the AnalysisDocument is available.
			HandleDocumentSaved(boundDoc, new EventArgs());
		}


		async void HandleDocumentSaved(object sender, EventArgs e)
		{
			SyntaxTree syntaxTree = null;
			SemanticModel semanticModel = null;
			if (boundDoc.FileName.Extension == ".cs" && boundDoc.AnalysisDocument != null)
			{
				syntaxTree = await boundDoc.AnalysisDocument.GetSyntaxTreeAsync();
				semanticModel = await boundDoc.AnalysisDocument.GetSemanticModelAsync();
			}

			Log.Information($"Document changed {boundDoc.Name}");
			DocumentChanged?.Invoke(this, new DocumentChangedEventArgs(boundDoc.FileName,
																	   boundDoc.Editor.Text,
																	   syntaxTree,
																	   semanticModel));
		}

		public Task RunTarget(string taskName)
		{
			// FIXME: This isn't working as expected, the XamlG target isn't called
			return boundDoc.Project.PerformGeneratorAsync(
				boundDoc.Project.ParentSolution.DefaultConfigurationSelector, taskName);
		}
	}
}
