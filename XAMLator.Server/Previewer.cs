using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace XAMLator.Server
{
    /// <summary>
    /// Previews requests sent by the IDE.
    /// </summary>
    public class Previewer : IPreviewer
    {
        bool presented;
        PreviewPage previewPage;
        ErrorPage errorPage;
        Dictionary<Type, object> viewModelsMapping;

        public Previewer(Dictionary<Type, object> viewModelsMapping)
        {
            this.viewModelsMapping = viewModelsMapping;
            var quitLive = new ToolbarItem
            {
                Text = "Quit live preview",
                Command = new Command(() =>
                {
                    Application.Current.MainPage.Navigation.PopModalAsync();
                    presented = false;
                }),
            };
            previewPage = new PreviewPage(quitLive);
            errorPage = new ErrorPage();
        }

        /// <summary>
        /// Preview the specified evaluation result.
        /// </summary>
        /// <param name="res">Res.</param>
        public async Task Preview(EvalResult res)
        {
            Log.Information($"Previewing {res.GetType()}");
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
                if (!presented)
                {
                    await Application.Current.MainPage.Navigation.PushModalAsync(previewPage, false);
                    presented = true;
                }
                NavigationPage.SetHasNavigationBar(previewPage, true);
                previewPage.ChangePage(page);
            }
        }

        public Task NotifyError(ErrorViewModel errorViewModel)
        {
            errorPage.BindingContext = errorViewModel;
            previewPage.ChangePage(errorPage);
            return Task.FromResult(true);
        }
    }
}