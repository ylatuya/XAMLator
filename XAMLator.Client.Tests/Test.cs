using NUnit.Framework;
using Microsoft.CodeAnalysis.MSBuild;
using System;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
//using Microsoft.CodeAnalysis.Workspace;
using System.IO;
using Buildalyzer;

namespace XAMLator.Client.Tests
{
	[TestFixture()]
	public class Test
	{
		public static void SetMonoMSBuildPath()
		{
			var assembly = typeof(System.String).Assembly;
			var assemblyDirectory = Path.GetDirectoryName(assembly.Location);
			var directoryInfo = new DirectoryInfo(assemblyDirectory).Parent;
			var msbuild = Path.Combine(directoryInfo.FullName, "msbuild", "15.0", "bin", "MSBuild.dll");

			Environment.SetEnvironmentVariable("MSBUILD_EXE_PATH", msbuild);
		}

		[Test()]
		public async Task TestCase()
		{
			SetMonoMSBuildPath();

			string solution_dir = Path.Combine(Path.GetDirectoryName(TestContext.CurrentContext.TestDirectory), "..", "..");


			var workspace = MSBuildWorkspace.Create();
			var projectFile = Path.Combine(solution_dir, "SampleApp/XAMLator.SampleApp.Gtk/XAMLator.SampleApp.Gtk.csproj");
			var project = await workspace.OpenProjectAsync(projectFile);

			AnalyzerManager manager = new AnalyzerManager();
			ProjectAnalyzer analyzer = manager.GetProject(projectFile);

			Console.WriteLine(project.Documents);
		}
	}
}
