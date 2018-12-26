using System;
using System.Threading.Tasks;

namespace XAMLator.Server.Tests
{
	public class TestUIToolkit : IUIToolkit
	{
		public void RunInUIThread(Action action)
		{
			action();
		}

		public Task RunInUIThreadAsync(Func<Task> action)
		{
			return action();
		}
	}
}
