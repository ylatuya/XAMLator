using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.LanguageServices;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using XAMLator.Client.VisualStudio;
using XAMLator.Client.VisualStudio.Helpers;
using XAMLator.Client.VisualStudio.Services;
using Task = System.Threading.Tasks.Task;

namespace XAMLator.Client
{
    [Guid(PackageGuidString)]
    [InstalledProductRegistration("#1110", "#1112", "1.0.3", IconResourceID = 1400)]
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [ProvideAutoLoad(UIContextGuids80.SolutionExists, PackageAutoLoadFlags.BackgroundLoad)]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "pkgdef, VS and vsixmanifest are valid VS terms")]
    public sealed class StartupHandler : AsyncPackage
    {
        public const string PackageGuidString = "9ea65290-6b43-425c-9aeb-328adcf096c9";

        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            IComponentModel componentModel = (IComponentModel)(await GetServiceAsync(typeof(SComponentModel)));
            RunningDocTableEvents runningDocTableEventListener = new RunningDocTableEvents();

            await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

            IVsRunningDocumentTable iVsRunningDocumentTable =
                (IVsRunningDocumentTable)GetGlobalService(typeof(SVsRunningDocumentTable));

            iVsRunningDocumentTable.AdviseRunningDocTableEvents(runningDocTableEventListener, out uint mRdtCookie);

            DocumentService documentService =
                new DocumentService(iVsRunningDocumentTable, runningDocTableEventListener, componentModel.GetService<VisualStudioWorkspace>());

            OutputWindowHelper.LogWriteLine("XAMLator initialized.");

            VisualStudioIDE visualStudioIDE = new VisualStudioIDE(documentService);
            XAMLatorMonitor.Init(visualStudioIDE);
            XAMLatorMonitor.Instance.StartMonitoring();

            OutputWindowHelper.LogWriteLine("XAMLator Start monitoring...");
        }
    }
}