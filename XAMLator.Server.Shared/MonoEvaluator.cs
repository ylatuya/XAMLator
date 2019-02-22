using System;
using System.Collections.Generic;
using System.Linq;
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
				Log.Error("Runtime evaluation not supported, did you set the mtouch option --enable-repl?");
				isEvaluationSupported = false;
			}
		}

		public bool IsEvaluationSupported => isEvaluationSupported;

		public Task<bool> EvaluateCode(string code, EvalResult result, string initCode = null)
		{
			if (string.IsNullOrEmpty(code))
			{
				return Task.FromResult(false);
			}

			EnsureConfigured();
			return Evaluate(code, result, initCode, true);
		}

		async Task<bool> Evaluate(string code, EvalResult result, string initCode, bool retryOnError)
		{
			try
			{
				printer.Reset();
				if (initCode != null)
				{
					eval.Evaluate(initCode, out object retResult, out bool result_set);
				}
				result.Result = eval.Evaluate(code);
				return true;
			}
			catch (Exception ex)
			{
				if (retryOnError)
				{
					eval = null;
					EnsureConfigured();
					return await Evaluate(code, result, initCode, false);
				}

				Log.Error($"Error evalutaing code");
				eval = null;
				if (printer.Messages.Count != 0)
				{
					result.Messages = printer.Messages.ToArray();
				}
				else
				{
					result.Messages = new[] { new EvalMessage("error", ex.ToString()) };
				}
				return false;
			}
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
			if (name == "mscorlib" || name == "System" || name == "System.Core" || name.StartsWith("eval-"))
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
