using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CodeAnalysis.Scripting.Hosting;

namespace XAMLator.Server
{
	/// Evaluates expressions using Roslyn's C# Scripint API.
	public class Evaluator : IEvaluator
	{
		static bool isEvaluationSupported;

		ScriptOptions options;

		static Evaluator()
		{
			try
			{
				CSharpScript.RunAsync("2+2").Wait();
				isEvaluationSupported = true;
			}
			catch (Exception ex)
			{
				isEvaluationSupported = false;
			}
		}

		public bool IsEvaluationSupported => isEvaluationSupported;

		public async Task<bool> EvaluateCode(string code, EvalResult result)
		{
			EnsureConfigured();
			try
			{
				var state = await CSharpScript.RunAsync(code);
				state = await state.ContinueWithAsync(evalExpression);
				result.Result = state.ReturnValue;
				return true;
			}
			catch (CompilationErrorException ex)
			{
				Log.Error($"Error evaluating {evalExpression}");
				result.Messages = new EvalMessage[] { new EvalMessage("error", ex.ToString()) };
			}
			return false;
		}

		void EnsureConfigured()
		{
			if (options == null)
			{
				ConfigureVM();
			}

		}

		void ConfigureVM()
		{
			var assemblies = AppDomain.CurrentDomain.GetAssemblies().Where(a => !a.IsDynamic).ToArray();
			options = ScriptOptions.Default.WithReferences(assemblies);
		}
	}
}
