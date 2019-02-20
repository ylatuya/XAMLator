using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Moq;
using NUnit.Framework;
using XAMLator.Client;

namespace XAMLator.Server.Tests
{
	public class XAMLatorFeatureBase
	{
		protected TestPreviewer previewer;
		protected TestWorkspace workspace;
		protected Mock<IIDE> ideMock;
		protected TestTCPCommunicator tcpCommunicator;
		protected TestUIToolkit uiToolkit;
		protected XAMLatorMonitor xamlatorMonitor;
		protected PreviewServer previewServer;
		string tempDir;

		[SetUp]
		public async Task Given_a_xamlator_server_listening()
		{
			SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());
			tempDir = GetTemporaryDirectory();
			workspace = new TestWorkspace(tempDir);
			ideMock = new Mock<IIDE>();
			tcpCommunicator = new TestTCPCommunicator();
			uiToolkit = new TestUIToolkit();

			xamlatorMonitor = new XAMLatorMonitor(ideMock.Object, tcpCommunicator);

			previewer = new TestPreviewer(new Dictionary<Type, object>());
			previewServer = new PreviewServer();
			await previewServer.RunInternal(null, previewer, null, Constants.DEFAULT_PORT, uiToolkit, tcpCommunicator);
		}

		[TearDown]
		public void TearDown()
		{
			FormsViewClassDeclaration.Reset();
			Directory.Delete(tempDir, true);
		}

		protected async Task When_a_csharp_document_changes(string docName)
		{
			var doc = workspace.FindDocument(docName);

			EmitDocumentChanged(doc.FilePath, "",
				await doc.GetSyntaxTreeAsync(), await doc.GetSemanticModelAsync());
		}

		protected async Task When_the_code_changes(string docName, string code)
		{
			Document doc = workspace.FindDocument(docName);
			workspace.UpdateDocument(ref doc, code);
			EmitDocumentChanged(doc.FilePath, code,
				await doc.GetSyntaxTreeAsync(), await doc.GetSemanticModelAsync());
		}

		protected void When_a_xaml_document_changes(string docName)
		{
			var doc = workspace.FindDocument(docName);
			EmitDocumentChanged(doc.FilePath, File.ReadAllText(doc.FilePath));
		}

		protected void When_a_xaml_document_changes(string docName, string xaml)
		{
			var doc = workspace.FindDocument(docName);
			EmitDocumentChanged(doc.FilePath, xaml);
		}

		protected void EmitDocumentChanged(string filePath, string content,
			SyntaxTree syntaxTree = null, SemanticModel semanticModel = null)
		{
			var args = new DocumentChangedEventArgs(filePath, content, syntaxTree, semanticModel);
			ideMock.Raise((IIDE obj) => obj.DocumentChanged += null, this, args);
		}

		string GetTemporaryDirectory()
		{
			string tempDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
			Directory.CreateDirectory(tempDirectory);
			return tempDirectory;
		}
	}
}
