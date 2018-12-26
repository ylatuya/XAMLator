using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;

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

		public async Task<bool> EvaluateCode(string code, EvalResult result, string initCode = null)
		{
			if (string.IsNullOrEmpty(code))
			{
				return false;
			}

			EnsureConfigured();
			try
			{
				ScriptState state;
				if (initCode != null)
				{
					state = await CSharpScript.RunAsync(initCode);
					await state.ContinueWithAsync(code);
				}
				else
				{
					state = await CSharpScript.RunAsync(code);
				}
				result.Result = state.ReturnValue;
			}
			catch (CompilationErrorException ex)
			{
				Log.Error($"Error evaluating code");
				result.Messages = new EvalMessage[] { new EvalMessage("error", ex.ToString()) };
				return false;
			}
			return true;
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
