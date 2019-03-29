using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.LanguageServices;
using Microsoft.VisualStudio.TextManager.Interop;
using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using XAMLator.Client.VisualStudio.Models;

namespace XAMLator.Client.VisualStudio.Services
{
    internal static class DocumentAnalysisHelper
    {
        public static Task<XAMLatorDocument> GetDocumentAsync(VisualStudioWorkspace workspace, string document, IntPtr documentData)
        {
            Task<XAMLatorDocument> result = null;

            switch (Path.GetExtension(document))
            {
                case ".css":
                    result = GetCssDocumentAsync(document, documentData);
                    break;
                default:
                    result = GetCodeDocumentAsync(workspace, document);
                    break;
            }

            return result;
        }

        private static Task<XAMLatorDocument> GetCssDocumentAsync(string document, IntPtr documentData)
        {
            string pbstrbuf = string.Empty;

            if (Marshal.GetObjectForIUnknown(documentData) is IVsTextLines iVsTextLines)
            {
                iVsTextLines.GetLastLineIndex(out int piLine, out int piIndex);
                iVsTextLines.GetLineText(0, 0, piLine, piIndex, out pbstrbuf);
            }

            return Task.FromResult(new XAMLatorDocument
            {
                Path = document,
                Code = pbstrbuf
            });
        }

        private static async Task<XAMLatorDocument> GetCodeDocumentAsync(VisualStudioWorkspace workspace, string path)
        {
            XAMLatorDocument result = null;

            if (workspace != null)
            {
                Solution solution = workspace.CurrentSolution;
                DocumentId documentId = solution.GetDocumentIdsWithFilePath(path).FirstOrDefault();
                Document document = solution.GetDocument(documentId);

                if (document != null)
                {
                    Task<SourceText> sourceTextTask = document.GetTextAsync();

                    result = new XAMLatorDocument
                    {
                        Path = path,
                        Code = (await sourceTextTask)?.ToString() ?? string.Empty,
                        SemanticModel = await document.GetSemanticModelAsync(),
                        SyntaxTree = await document.GetSyntaxTreeAsync()
                    };
                }
            }

            return result;
        }
    }
}