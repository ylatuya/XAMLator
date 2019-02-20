using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace XAMLator.Server.Tests
{
	public enum PreviewState
	{
		None,
		Error,
		Preview
	}

	public class TestPreviewer : Previewer
	{
		public TestPreviewer(Dictionary<Type, object> viewModelsMapping) : base(viewModelsMapping) { }

		public PreviewState State { get; private set; }

		public ErrorViewModel ErrorViewModel { get; private set; }

		public EvalResult EvalResult { get; private set; }

		public Page PreviewedPage { get; private set; }

		public override Task NotifyError(ErrorViewModel errorViewModel)
		{
			State = PreviewState.Error;
			ErrorViewModel = errorViewModel;
			EvalResult = null;
			return base.NotifyError(errorViewModel);
		}

		public override Task Preview(EvalResult res)
		{
			State = PreviewState.Preview;
			ErrorViewModel = null;
			EvalResult = res;
			return base.Preview(res);
		}

		protected override Task PreviewPage(Page page)
		{
			PreviewedPage = page;
			return Task.FromResult(true);
		}

		protected override Task HidePreviewPage(Page previewPage)
		{
			return Task.FromResult(true);
		}

		protected override Task ShowPreviewPage(Page previewPage)
		{
			PreviewedPage = previewPage;
			return Task.FromResult(true);
		}
	}
}
