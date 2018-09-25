using System;
using System.Collections.Generic;
using Mono.CSharp;

namespace XAMLator.Server
{
	public partial class VM
	{
		class Printer : ReportPrinter
		{
			public readonly List<EvalMessage> Messages = new List<EvalMessage>();
			public override void Print(AbstractMessage msg, bool showFullPath)
			{
				var line = 0;
				var column = 0;
				try
				{
					line = msg.Location.Row;
					column = msg.Location.Column;
				}
				catch
				{
					//Log (ex);
				}
				var m = new EvalMessage
				{
					MessageType = msg.MessageType,
					Text = msg.Text,
					Line = line,
					Column = column,
				};

				Messages.Add(m);

				//
				// Print it to the console if there's an error
				//
				if (msg.MessageType == "error")
				{
					var tm = msg.Text;
					System.Threading.ThreadPool.QueueUserWorkItem(_ =>
					   Console.WriteLine("ERROR: {0}", tm));
				}
			}
			public void AddError(Exception ex)
			{
				var text = ex.ToString();

				var m = new EvalMessage
				{
					MessageType = "error",
					Text = text,
					Line = 0,
					Column = 0,
				};

				Messages.Add(m);

				//
				// Print it to the console if there's an error
				//
				System.Threading.ThreadPool.QueueUserWorkItem(_ =>
				   Console.WriteLine("EVAL ERROR: {0}", text));
			}
		}
	}
}

