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
		ScriptOptions options;

		public async Task<bool> CreateNewTypeInstance(string typeName, string code, EvalResult result)
		{
			EnsureConfigured();
			try
			{
				var state = await CSharpScript.RunAsync(code);
				state = await state.ContinueWithAsync($"new {typeName}()");
				result.Result = state.ReturnValue;
				return true;
			}
			catch (CompilationErrorException ex)
			{
				Log.Error($"Error evaluating new instance for type {typeName}");
				result.Messages = new EvalMessage[] { new EvalMessage {
						MessageType = "error", Text = ex.ToString()}
				};
			}
			return false;
		}

		public async Task<bool> CreateTypeInstance(string typeName, EvalResult result)
		{
			EnsureConfigured();
			try
			{
				using (var loader = new InteractiveAssemblyLoader())
				{
					var script = CSharpScript.Create($"new {typeName} ()", options, assemblyLoader: loader);
					var state = await script.RunAsync();
					result.Result = state.ReturnValue;
				}

			}
			catch (CompilationErrorException ex)
			{
				Log.Exception(ex);
				Log.Error($"Error evaluating new instance of {typeName}");
				result.Messages = new EvalMessage[] { new EvalMessage {
						MessageType = "error", Text = ex.ToString()}
				};
			}
			catch (Exception ex)
			{
				Log.Exception(ex);
				Log.Error($"Unhandled rrror creating new instance of {typeName}");
				result.Messages = new EvalMessage[] { new EvalMessage {
						MessageType = "error", Text = ex.ToString()}
				};
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
