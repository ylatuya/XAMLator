using Xamarin.Forms;

namespace XAMLator.Server
{
	/// <summary>
	/// A Page to preview XAML updates.
	/// </summary>
	public class PreviewPage : MultiPage<Page>
	{

		public PreviewPage(ToolbarItem backButton)
		{
			ToolbarItems.Add(backButton);
			NavigationPage.SetHasNavigationBar(this, true);
		}

		public void ChangePage(Page page)
		{
			this.Children.Clear();
			this.Children.Add(page);
			CurrentPage = page;
			NavigationPage.SetHasNavigationBar(CurrentPage, true);
		}

		protected override Page CreateDefault(object item)
		{
			return null;
		}

		protected override void LayoutChildren(double x, double y, double width, double height)
		{
			base.LayoutChildren(x, y, width, height);
		}
	}
}
