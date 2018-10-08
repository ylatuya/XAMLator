using System;
using MonoDevelop.Components.Commands;

namespace XAMLator.Client
{
	public class StartupHandler : CommandHandler
	{
		protected override void Run()
		{
			XAMLatorMonitor.Init(new MonoDevelopIDE());
			XAMLatorMonitor.Instance.StartMonitoring();
		}
	}
}
