using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Mono.CSharp;

namespace XAMLator.Server
{
	public class Evaluator : IEvaluator
	{
		static readonly bool isEvaluationSupported;

		Mono.CSharp.Evaluator eval;
		Printer printer;

		static Evaluator()
		{
			var eval = new Mono.CSharp.Evaluator(new CompilerContext(new CompilerSettings(), new Printer()));
			try
			{
				eval.Evaluate("2+2");
				isEvaluationSupported = true;
			}
			catch (Exception ex)
			{
				isEvaluationSupported = false;
			}
		}

		public bool IsEvaluationSupported => isEvaluationSupported;

		public Task<bool> EvaluateCode(string code, EvalResult result)
		{
			EnsureConfigured();
			try
			{
				object retResult;
				bool hasResult;

				printer.Reset();
				if (!String.IsNullOrEmpty(code))
				{
					var ret = eval.Evaluate(code, out retResult, out hasResult);
				}
				result.Result = eval.Evaluate(expression);
				return Task.FromResult(true);
			}
			catch (Exception ex)
			{
				Log.Error($"Error creating a new instance of {expression}");
				if (printer.Messages.Count != 0)
				{
					result.Messages = printer.Messages.ToArray();
				}
				else
				{
					result.Messages = new EvalMessage[] { new EvalMessage("error", ex.ToString()) };
				}
				if (!result.HasResult && result.Messages.Length == 0)
				{
					result.Messages = new EvalMessage[] { new EvalMessage("error", "Internal Error") };
				}
				eval = null;
			}
			return Task.FromResult(false);
		}

		void EnsureConfigured()
		{
			if (eval != null)
			{
				return;
			}

			var settings = new CompilerSettings();
			printer = new Printer();
			var context = new CompilerContext(settings, printer);
			eval = new Mono.CSharp.Evaluator(context);
			AppDomain.CurrentDomain.AssemblyLoad += (_, e) =>
			{
				LoadAssembly(e.LoadedAssembly);
			};
			foreach (var a in AppDomain.CurrentDomain.GetAssemblies())
			{
				LoadAssembly(a);

			}
		}

		void LoadAssembly(Assembly assembly)
		{
			var name = assembly.GetName().Name;
			if (name == "mscorlib" || name == "System" || name == "System.Core")
				return;
			eval?.ReferenceAssembly(assembly);
		}
	}

	class Printer : ReportPrinter
	{
		public readonly List<EvalMessage> Messages = new List<EvalMessage>();

		public new void Reset()
		{
			Messages.Clear();
			base.Reset();
		}

		public override void Print(AbstractMessage msg, bool showFullPath)
		{
			if (msg.MessageType != "error")
			{
				return;
			}
			AddMessage(msg.MessageType, msg.Text, msg.Location.Row, msg.Location.Column);
		}

		public void AddError(Exception ex)
		{
			AddMessage("error", ex.ToString(), 0, 0);
		}

		void AddMessage(string messageType, string text, int line, int column)
		{
			var m = new EvalMessage(messageType, text, line, column);
			Messages.Add(m);
			if (m.MessageType == "error")
			{
				Log.Error(m.Text);
			}
		}
	}
}
