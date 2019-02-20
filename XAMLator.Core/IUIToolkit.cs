using System;
using System.Threading.Tasks;

namespace XAMLator
{
	public interface IUIToolkit
	{
		Task RunInUIThreadAsync(Func<Task> action);

		void RunInUIThread(Action action);
	}
}
