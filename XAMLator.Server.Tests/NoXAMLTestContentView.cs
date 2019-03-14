using Xamarin.Forms;

namespace XAMLator.Server.Tests
{
	public class NoXAMLTestContentView : ContentView
	{
		public NoXAMLTestContentView()
		{
			Content = new Label { Text = "Hello ContentView" };
		}
	}
}

