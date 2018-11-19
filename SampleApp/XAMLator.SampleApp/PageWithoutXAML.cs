using System;

using Xamarin.Forms;

namespace XAMLator.SampleApp
{
	public class PageWithoutXAML : ContentPage
	{
		public PageWithoutXAML()
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

