using Xamarin.Forms;

namespace XAMLator.Server.Tests
{
	public class NoXAMLTestContentPage : ContentPage
	{
		public NoXAMLTestContentPage()
		{
			Content = new StackLayout
			{
				Children = {
					new Label { Text = "Hello ContentPage" }
				}
			};
		}
	}
}

