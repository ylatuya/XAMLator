using System;
using System.Reflection;
using System.Threading.Tasks;
using Mono.CSharp;

namespace XAMLator.Server
{
	public class Evaluator : IEvaluator
	{
		Mono.CSharp.Evaluator eval;

		public Task<bool> CreateNewTypeInstance(string typeName, string code, EvalResult result)
		{
			throw new NotImplementedException();
		}

		public Task<bool> CreateTypeInstance(string typeName, EvalResult result)
		{
			EnsureConfigured();
			try
			{
				result.Result = eval.Evaluate($"new {typeName} ()");
				return Task.FromResult(true);
			}
			catch (Exception ex)
			{
				Log.Exception(ex);
				Log.Error($"Unhandled error creating new instance of {typeName}");
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
			eval.ReferenceAssembly(assembly);
		}
	}
}
