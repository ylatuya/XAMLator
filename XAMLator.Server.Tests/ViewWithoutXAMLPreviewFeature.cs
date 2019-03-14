using System;
using System.Threading.Tasks;
using NUnit.Framework;
using Xamarin.Forms;

namespace XAMLator.Server.Tests
{
	[TestFixture]
	public class ViewWithoutXAMLPreviewFeature : XAMLatorFeatureBase
	{
		[Test]
		public async Task The_user_changes_a_content_page_class_without_xaml_and_its_previewed()
		{
			await When_the_code_changes("NoXAMLTestContentPage.cs", @"
               using Xamarin.Forms;
               namespace XAMLator.Server.Tests {
                   public class NoXAMLTestContentPage : ContentPage {
                       public NoXAMLTestContentPage() {
                           Content = new Label { Text = ""Hello ContentPage"" };
		              }
                   }
               }");
			var label = (previewer.PreviewedPage as ContentPage).Content as Label;
			Assert.AreEqual(PreviewState.Preview, previewer.State);
			Assert.AreEqual("Hello ContentPage", label.Text);
		}

		[Test]
		public async Task The_user_changes_a_conent_view_class_without_xaml_and_its_previewed()
		{
			await When_the_code_changes("NoXAMLTestContentView.cs", @"
               using Xamarin.Forms;
               namespace XAMLator.Server.Tests {
                   public class NoXAMLTestContentView : ContentView {
                       public NoXAMLTestContentView() {
                           Content = new Label { Text = ""Hello!"" };
		              }
                   }
               }");
			var label = ((previewer.PreviewedPage as ContentPage).Content as ContentView).Content as Label;
			Assert.AreEqual(PreviewState.Preview, previewer.State);
			Assert.AreEqual("Hello!", label.Text);
		}

		[Test]
		public async Task The_user_changes_a_view_page_class_with_inheritance_without_xaml_and_its_previewed()
		{
			await When_the_code_changes("NoXAMLTestInheritanceContentView.cs", @"
               using Xamarin.Forms;
               namespace XAMLator.Server.Tests {
                   public class NoXAMLTestInheritanceContentView : NoXAMLTestContentView {
                       public NoXAMLTestInheritanceContentView() {
		              }
                   }
               }");
			var label = ((previewer.PreviewedPage as ContentPage).Content as ContentView).Content as Label;
			Assert.AreEqual(PreviewState.Preview, previewer.State);
			Assert.AreEqual("Hello ContentView", label.Text);
		}
	}
}
