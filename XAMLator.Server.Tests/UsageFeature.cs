using System;
using System.Threading.Tasks;
using NUnit.Framework;

namespace XAMLator.Server.Tests
{
	[TestFixture]
	public class UsageFeature : XAMLatorFeatureBase
	{
		[Test]
		public async Task A_xaml_changes_after_a_code_behind_and_the_view_is_previewed()
		{
			await When_a_csharp_document_changes("TestPage.xaml.cs");
			When_a_xaml_document_changes("TestPage.xaml");
			Assert.AreEqual(PreviewState.Preview, previewer.State);
			Assert.AreEqual("XAMLator.Server.Tests.TestPage1", previewer.EvalResult.ResultType.FullName);
		}

		[Test]
		public async Task A_code_behind_changes_after_xaml_and_the_view_is_previewed()
		{
			When_a_xaml_document_changes("TestPage.xaml");
			await When_a_csharp_document_changes("TestPage.xaml.cs");
			Assert.AreEqual(PreviewState.Preview, previewer.State);
			Assert.AreEqual("XAMLator.Server.Tests.TestPage1", previewer.EvalResult.ResultType.FullName);
		}

	}
}
