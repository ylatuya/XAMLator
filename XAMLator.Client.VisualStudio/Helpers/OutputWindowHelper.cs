using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;

namespace XAMLator.Client.VisualStudio.Helpers
{
    internal static class OutputWindowHelper
    {
        public const string OutputWindowGuidString = "cc2b124c-73e0-4314-8969-7b21e759cdc0";

        private static IVsOutputWindowPane _xamlatorVSOutputWindowPane;

        private static IVsOutputWindowPane GiteaVSOutputWindowPane =>
            _xamlatorVSOutputWindowPane ?? (_xamlatorVSOutputWindowPane = GetXAMLatorVsOutputWindowPane());

        internal static void LogWriteLine(string message)
        {
            WriteLine("Log", message);
        }

        internal static void DiagnosticWriteLine(string message, Exception ex = null)
        {
            if (ex != null)
            {
                message += $": {ex}";
            }

            WriteLine("Diagnostic", message);
        }

        internal static void ExceptionWriteLine(string message, Exception ex)
        {
            var exceptionMessage = $"{message}: {ex}";
            WriteLine("Handled Exception", exceptionMessage);
        }

        internal static void WarningWriteLine(string message)
        {
            WriteLine("Warning", message);
        }

        private static IVsOutputWindowPane GetXAMLatorVsOutputWindowPane()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            IVsOutputWindow outputWindow = Package.GetGlobalService(typeof(SVsOutputWindow)) as IVsOutputWindow;
            if (outputWindow == null) return null;

            Guid outputPaneGuid = new Guid(OutputWindowGuidString);

            outputWindow.CreatePane(ref outputPaneGuid, "XAMLator for Visual Studio", 1, 1);
            outputWindow.GetPane(ref outputPaneGuid, out IVsOutputWindowPane windowPane);

            return windowPane;
        }

        private static void WriteLine(string category, string message)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var outputWindowPane = GiteaVSOutputWindowPane;
            if (outputWindowPane != null)
            {
                string outputMessage = $"[XAMLator for Visual Studio  {category} {DateTime.Now.ToString("hh:mm:ss tt")}] {message}{Environment.NewLine}";
                outputWindowPane.OutputString(outputMessage);
            }
        }
    }
}