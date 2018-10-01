using System;
using System.Reflection;
using System.Threading.Tasks;
using Mono.CSharp;

namespace XAMLator.Server
{
	public class Evaluator : IEvaluator
	{
		Mono.CSharp.Evaluator eval;

		public Task<bool> CreateNewTypeInstance(string expression, string code, EvalResult result)
		{
			EnsureConfigured();
			try
			{
				object retResult;
				bool hasResult;
				if (!String.IsNullOrEmpty(code))
				{
					var ret = eval.Evaluate(code, out retResult, out hasResult);
				}
				result.Result = eval.Evaluate(expression);
				return Task.FromResult(true);
			}
			catch (Exception ex)
			{
				Log.Exception(ex);
				Log.Error($"Unhandled error creating new instance of {expression}");
				result.Messages = new EvalMessage[] { new EvalMessage {
						MessageType = "error", Text = ex.ToString()}
				};
			}
			return Task.FromResult(false);
		}

		public Task<bool> EvaluateExpression(string expression, EvalResult result)
		{
			EnsureConfigured();
			try
			{
				result.Result = eval.Evaluate($"new {expression} ()");
				return Task.FromResult(true);
			}
			catch (Exception ex)
			{
				Log.Exception(ex);
				Log.Error($"Unhandled error creating new instance of {expression}");
				result.Messages = new EvalMessage[] { new EvalMessage {
						MessageType = "error", Text = ex.ToString()}
				};
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
			var context = new CompilerContext(settings, new ConsoleReportPrinter());
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
}
