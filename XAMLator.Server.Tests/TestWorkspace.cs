using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using NUnit.Framework;

namespace XAMLator.Server.Tests
{
	public class TestWorkspace : IDisposable
	{
		AdhocWorkspace workspace;

		public TestWorkspace(string tempDir)
		{
			CopyFile(TestProjectDir, "TestPage.xaml", tempDir);
			CopyFile(TestProjectDir, "TestPage.xaml.cs", tempDir);
			CopyFile(TestAutogenDir, "TestPage.xaml.g.cs", tempDir);
			CopyFile(TestProjectDir, "NoXAMLTestContentPage.cs", tempDir);
			CopyFile(TestProjectDir, "NoXAMLTestContentView.cs", tempDir);
			CopyFile(TestProjectDir, "NoXAMLTestInheritanceContentView.cs", tempDir);
			workspace = new AdhocWorkspace();
			var solution = SolutionInfo.Create(SolutionId.CreateNewId(), VersionStamp.Default);
			workspace.AddSolution(solution);
			var projectId = ProjectId.CreateNewId();
			var versionStamp = VersionStamp.Create();
			var projectInfo = ProjectInfo.Create(projectId, versionStamp, "TestProject", "TestProject", LanguageNames.CSharp);
			var testProject = workspace.AddProject(projectInfo);
			AddReferences(testProject);
			AddDocument(testProject, tempDir, "TestPage.xaml.cs");
			AddDocument(testProject, tempDir, "TestPage.xaml");
			AddDocument(testProject, tempDir, "TestPage.xaml.g.cs");
			AddDocument(testProject, tempDir, "NoXAMLTestContentPage.cs");
			AddDocument(testProject, tempDir, "NoXAMLTestContentView.cs");
			AddDocument(testProject, tempDir, "NoXAMLTestInheritanceContentView.cs");
			// Define a stub type for ContentPage and ConentView so they resolve to a type and not to an ErrorType
			// as the Xamarin.Forms types are not defined in this workspae
			AddDocument(testProject, tempDir, "ContentPage.cs", @"namespace Xamarin.Forms { class ContentPage: Page {}}");
			AddDocument(testProject, tempDir, "ContentPage.cs", @"namespace Xamarin.Forms { class ContentView: View {}}");
		}

		public void Dispose()
		{
			workspace.Dispose();
		}

		public Document TestPageDoc => FindDocument("TestPage.xaml.cs");

		string TestProjectDir => Path.Combine(TestContext.CurrentContext.TestDirectory, "..", "..");

#if DEBUG
		string TestAutogenDir => Path.Combine(TestProjectDir, "obj", "Debug");
#else
		string TestAutogenDir => Path.Combine(TestProjectDir, "obj", "Release");
#endif

		public Document FindDocument(string docName)
		{
			return workspace.CurrentSolution.Projects.Single().Documents.Single(d => d.Name == docName);
		}

		public void UpdateDocument(ref Document document, string code)
		{
			var newSolution = workspace.CurrentSolution.WithDocumentText(
				document.Id, SourceText.From(code), PreservationMode.PreserveIdentity);
			workspace.TryApplyChanges(newSolution);
			document = workspace.CurrentSolution.GetDocument(document.Id);
		}

		void CopyFile(string dir, string fileName, string outDir)
		{
			File.Copy(Path.Combine(dir, fileName), Path.Combine(outDir, fileName), true);
		}

		Document AddDocument(Project testProject, string dir, string fileName)
		{
			return AddDocument(testProject, dir, fileName, ReadFile(dir, fileName));
		}

		Document AddDocument(Project testProject, string dir, string fileName, string text)
		{
			return AddDocument(testProject, dir, fileName, SourceText.From(text));
		}

		Document AddDocument(Project testProject, string dir, string fileName, SourceText content)
		{
			DocumentInfo documentInfo = DocumentInfo.Create(
				DocumentId.CreateNewId(testProject.Id),
				fileName,
				new List<string> { dir },
				SourceCodeKind.Regular,
				TextLoader.From(TextAndVersion.Create(content, VersionStamp.Create())),
				Path.Combine(dir, fileName));
			return workspace.AddDocument(documentInfo);
		}

		SourceText ReadFile(string dir, string fileName)
		{
			return SourceText.From(File.ReadAllText(Path.Combine(dir, fileName)));
		}

		void AddReferences(Project project)
		{
			foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
			{
				if (!assembly.IsDynamic)
				{
					project.AddMetadataReference(MetadataReference.CreateFromFile(assembly.Location));
				}
			}
		}
	}
}