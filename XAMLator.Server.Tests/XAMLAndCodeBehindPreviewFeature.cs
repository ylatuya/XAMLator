using System.Threading.Tasks;
using NUnit.Framework;
using Xamarin.Forms;

namespace XAMLator.Server.Tests
{
	[TestFixture]
	public class XAMLAndCodeBehindPreviewFeature : XAMLatorFeatureBase
	{
		[Test]
		public async Task A_code_behind_changes_the_first_time_and_the_view_is_previewed_with_a_new_type()
		{
			await When_a_csharp_document_changes("TestPage.xaml.cs");
			Assert.AreEqual(PreviewState.Preview, previewer.State);
			Assert.AreEqual("XAMLator.Server.Tests.TestPage1", previewer.EvalResult.ResultType.FullName);
		}

		[Test]
		public async Task When_the_code_changes_the_text_of_a_label_the_view_is_updated()
		{
			await When_the_code_changes("TestPage.xaml.cs", @"
			    using Xamarin.Forms;
                namespace XAMLator.Server.Tests{
                    public partial class TestPage : ContentPage {
	              	    public TestPage() {
			                InitializeComponent();
                            label.Text = ""NEW TEXT"";
	                    }}}");
			var label = (previewer.PreviewedPage as ContentPage).Content as Label;
			Assert.AreEqual(PreviewState.Preview, previewer.State);
			Assert.AreEqual("NEW TEXT", label.Text);
		}

		[Test]
		public async Task When_the_code_contains_errors_the_error_page_is_shown()
		{
			await When_the_code_changes("TestPage.xaml.cs", @"
			    using Xamarin.Forms;
                namespace XAMLator.Server.Tests{
                    public partial class TestPage : ContentPage {
	              	    public TestPage() {
			                InvalidMethod();
	                    }}}");
			Assert.AreEqual(PreviewState.Error, previewer.State);
			Assert.AreEqual("Oh no! An evaluation error!", previewer.ErrorViewModel.Title);
		}

		[Test]
		public async Task When_the_code_is_not_parsable_the_error_page_is_shown()
		{
			await When_the_code_changes("TestPage.xaml.cs", @"public new {}");
			Assert.AreEqual(PreviewState.Error, previewer.State);
			Assert.AreEqual("Oh no! An exception!", previewer.ErrorViewModel.Title);
		}
	}
}