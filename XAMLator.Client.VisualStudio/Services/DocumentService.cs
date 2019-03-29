using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.LanguageServices;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Threading;
using XAMLator.Client.VisualStudio.Helpers;
using XAMLator.Client.VisualStudio.Models;

namespace XAMLator.Client.VisualStudio.Services
{
    public class DocumentService
    {
        private readonly IVsRunningDocumentTable _iVsRunningDocumentTable;
        private readonly VisualStudioWorkspace _workspace;

        public EventHandler<XAMLatorDocument> OnDocumentChanged;

        public DocumentService(
            IVsRunningDocumentTable iVsRunningDocumentTable,
            RunningDocTableEvents runningDocTableEvents,
            VisualStudioWorkspace workspace)
        {
            _iVsRunningDocumentTable = iVsRunningDocumentTable;
            _workspace = workspace;

            runningDocTableEvents.AfterSave += OnAfterSave;
        }

        private async void OnAfterSave(object sender, uint docCookie)
        {
            await OnFileSavedAsync(sender, docCookie);
        }

        private async System.Threading.Tasks.Task OnFileSavedAsync(object sender, uint docCookie)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            (string document, IntPtr documentData) = GetDocumentInfo(docCookie);

            await TaskScheduler.Default;

            XAMLatorDocument analysis = await DocumentAnalysisHelper.GetDocumentAsync(_workspace, document, documentData);

            if (analysis != null)
                OnDocumentChanged?.Invoke(this, analysis);
        }

        private (string document, IntPtr documentData) GetDocumentInfo(uint docCookie)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            string pbstrMkDocument = string.Empty;

            _iVsRunningDocumentTable.GetDocumentInfo(docCookie, out uint pgrfRdtFlags, out uint pdwReadLocks,
                out uint pdwEditLocks, out pbstrMkDocument, out IVsHierarchy ppHier, out uint pitemid,
                out IntPtr ppunkDocData);

            return (document: pbstrMkDocument, documentData: ppunkDocData);
        }
    }
}