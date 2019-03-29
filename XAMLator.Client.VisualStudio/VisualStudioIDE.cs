using System;
using XAMLator.Client.VisualStudio.Helpers;
using XAMLator.Client.VisualStudio.Models;
using XAMLator.Client.VisualStudio.Services;
using Task = System.Threading.Tasks.Task;

namespace XAMLator.Client.VisualStudio
{
    public class VisualStudioIDE : IIDE
    {
        public event EventHandler<DocumentChangedEventArgs> DocumentChanged;

        public VisualStudioIDE(DocumentService documentService)
        {
            documentService.OnDocumentChanged += OnDocumentChanged;
        }

        public void MonitorEditorChanges()
        {
            OutputWindowHelper.LogWriteLine("Monitor editor changes.");
        }

        public void ShowError(string error, Exception ex = null)
        {
            OutputWindowHelper.ExceptionWriteLine(error, ex);
        }

        public Task RunTarget(string targetName)
        {
            return Task.CompletedTask;
        }

        private void OnDocumentChanged(object sender, XAMLatorDocument documentAnalysis)
        {
            if (documentAnalysis != null)
            {
                DocumentChangedEventArgs documentChangedEventArgs = new DocumentChangedEventArgs(documentAnalysis.Path,
                    documentAnalysis.Code, documentAnalysis.SyntaxTree, documentAnalysis.SemanticModel);

                DocumentChanged?.Invoke(this, documentChangedEventArgs);
            }
        }
    }
}