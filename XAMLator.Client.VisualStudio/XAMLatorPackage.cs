using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Threading;
using EnvDTE;
using Microsoft;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Task = System.Threading.Tasks.Task;

namespace XAMLator.Client
{
    [Guid(PackageGuidString)]
    [InstalledProductRegistration("#1110", "#1112", "1.0.3", IconResourceID = 1400)]
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [ProvideAutoLoad("{ADFC4E64-0397-11D1-9F4E-00A0C911004F}")]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "pkgdef, VS and vsixmanifest are valid VS terms")]
    public sealed class XAMLatorPackage : AsyncPackage, IIDE
    {
        public const string PackageGuidString = "9ea65290-6b43-425c-9aeb-328adcf096c9";

        private DTE _application;
        private SolutionEvents _solutionEvents;
        private DocumentEvents _documentEvents;

        public event EventHandler<DocumentChangedEventArgs> DocumentChanged;

        public void MonitorEditorChanges()
        {
            Logger.Log("Monitor editor changes.");
        }

        public void ShowError(string error, Exception ex = null)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            IVsUIShell shellService = GetService(typeof(SVsUIShell)) as IVsUIShell;
            Assumes.Present(shellService);

            shellService.ShowMessageBox(
              0,
              Guid.Empty,
              "XAMLator",
              error,
              string.Empty,
              0,
              OLEMSGBUTTON.OLEMSGBUTTON_OK,
              OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST,
              OLEMSGICON.OLEMSGICON_CRITICAL,
              0,      
              out int result);

            if(ex != null)
                Logger.Log(ex);
        }

        public Task RunTarget(string targetName)
        {
            // TODO:
            return Task.FromResult(false);
        }

        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            Logger.Initialize(this, "XAMLator");

            //await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
            DTE serviceAsync = (DTE)await base.GetServiceAsync(typeof(SDTE));
            Assumes.Present(serviceAsync);

            _documentEvents = serviceAsync.Events.DocumentEvents;
            _application = serviceAsync.Application;
            _solutionEvents = _application.Events.SolutionEvents;

            _solutionEvents.Opened += OnSolutionOpened;
            _solutionEvents.AfterClosing += OnSolutionAfterClosing;

            Logger.Log("XAMLator initialized.");

            XAMLatorMonitor.Init(this);
            XAMLatorMonitor.Instance.StartMonitoring();

            Logger.Log("XAMLator Start monitoring...");
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                _solutionEvents.Opened -= OnSolutionOpened;
                _solutionEvents.AfterClosing -= OnSolutionAfterClosing;
            }
        }

        private void OnSolutionOpened()
        {
            _documentEvents.DocumentSaved += OnDocumentSaved;
        }

        private void OnSolutionAfterClosing()
        {
            _documentEvents.DocumentSaved -= OnDocumentSaved;
        }

        private async void OnDocumentSaved(Document document)
        {
            //await JoinableTaskFactory.SwitchToMainThreadAsync(DisposalToken);

            try
            {
                TextDocument textDocument = (TextDocument)document.Object("TextDocument");

                string documentName = System.IO.Path.Combine(document.Path, document.Name);
                string documentContent = textDocument.StartPoint.CreateEditPoint().GetText(textDocument.EndPoint);

                Microsoft.CodeAnalysis.SyntaxTree syntaxTree = null;
                Microsoft.CodeAnalysis.SemanticModel semanticModel = null;

                DocumentChanged?.Invoke(this, new DocumentChangedEventArgs(
                    documentName,
                    documentContent,
                    syntaxTree,
                    semanticModel));

                Logger.Log("Document saved.");
            }
            catch(Exception ex)
            {
                Logger.Log(ex);
            }
        }
    }
}