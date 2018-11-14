using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace XAMLator.Server
{
	/// <summary>
	/// Previews requests sent by the IDE.
	/// </summary>
	public class Previewer : IPreviewer
	{
		protected PreviewPage previewPage;
		protected ErrorPage errorPage;
		protected Dictionary<Type, object> viewModelsMapping;
		bool presented;
		ICommand closeCommand;

		public Previewer(Dictionary<Type, object> viewModelsMapping)
		{
			this.viewModelsMapping = viewModelsMapping;
			errorPage = new ErrorPage();
			closeCommand = new Command(() =>
			{
				HidePreviewPage(previewPage);
				presented = false;
			});
			var quitLive = new ToolbarItem
			{
				Text = "Quit live preview",
				Command = closeCommand
			};
			previewPage = new PreviewPage(quitLive);
		}

		public ICommand CloseCommand => closeCommand;

		/// <summary>
		/// Preview the specified evaluation result.
		/// </summary>
		/// <param name="res">Res.</param>
		public virtual async Task Preview(EvalResult res)
		{
			Log.Information($"Visualizing result {res.Result}");
			Page page = res.Result as Page;
			if (page == null && res.Result is View view)
			{
				page = new ContentPage { Content = view };
			}
			if (page != null)
			{
				if (viewModelsMapping.TryGetValue(res.Result.GetType(), out object viewModel))
				{
					page.BindingContext = viewModel;
				}
				await EnsurePresented();
				NavigationPage.SetHasNavigationBar(previewPage, true);
				previewPage.ChangePage(page);
			}
		}

		public virtual async Task NotifyError(ErrorViewModel errorViewModel)
		{
			await EnsurePresented();
			errorPage.BindingContext = errorViewModel;
			previewPage.ChangePage(errorPage);
		}

		protected virtual Task ShowPreviewPage(Page previewPage)
		{
			return Application.Current.MainPage.Navigation.PushModalAsync(previewPage, false);
		}

		protected virtual Task HidePreviewPage(Page previewPage)
		{
			return Application.Current.MainPage.Navigation.PopModalAsync();
		}

		protected async Task EnsurePresented()
		{
			if (!presented)
			{
				await ShowPreviewPage(previewPage);
				presented = true;
			}
		}
	}
}
