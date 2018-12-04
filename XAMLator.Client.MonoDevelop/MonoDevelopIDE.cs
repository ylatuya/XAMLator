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
				boundDoc = null;
			}

			var ext = doc?.FileName.Extension;
			if (ext == ".xaml" || ext == ".cs")
			{
				boundDoc = doc;
				Log.Information($"Monitoring document {boundDoc.FileName}");
				boundDoc.Saved += HandleDocumentSaved;
				EmitDocumentChanged();
			}
		}

		void EmitDocumentChanged(SyntaxTree syntaxTree = null, SemanticModel semanticModel = null)
		{
			Log.Information($"XAML document changed {boundDoc.Name}");
			DocumentChanged?.Invoke(this, new DocumentChangedEventArgs(boundDoc.FileName,
																	   boundDoc.Editor.Text,
																	   syntaxTree,
																	   semanticModel));
		}

		async void HandleDocumentSaved(object sender, EventArgs e)
		{
			EmitDocumentChanged(await boundDoc.AnalysisDocument.GetSyntaxTreeAsync(),
					 await boundDoc.AnalysisDocument.GetSemanticModelAsync());
		}

		public Task RunTarget(string taskName)
		{
			// FIXME: This isn't working as expected, the XamlG target isn't called
			return boundDoc.Project.PerformGeneratorAsync(
				boundDoc.Project.ParentSolution.DefaultConfigurationSelector, taskName);
		}
	}
}
