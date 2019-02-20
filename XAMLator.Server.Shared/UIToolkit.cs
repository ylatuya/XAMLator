using System;
using System.Threading.Tasks;

namespace XAMLator.Server
{
	public class UIToolkit : IUIToolkit
	{
		public void RunInUIThread(Action action)
		{
			Xamarin.Forms.Device.BeginInvokeOnMainThread(action);
		}

		public Task RunInUIThreadAsync(Func<Task> action)
		{
			var tcs = new TaskCompletionSource<bool>();
			Action asyncAction = () =>
			{
				try
				{
					action().ContinueWith((r) => tcs.SetResult(true));
				}
				catch (Exception ex)
				{
					tcs.SetException(ex);
				}
			};
			Xamarin.Forms.Device.BeginInvokeOnMainThread(asyncAction);
			return tcs.Task;
		}
	}
}
