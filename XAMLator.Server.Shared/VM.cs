using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace XAMLator.Server
{
	/// <summary>
	/// Loads XAML views live with requests from the IDE.
	/// </summary>
	public class VM
	{
		static MethodInfo loadXAML;
		static string currentXAML;
		readonly object mutex = new object();
		IEvaluator evaluator;

		static VM()
		{
			ResolveLoadMethod();
		}

		public VM()
		{
			evaluator = new Evaluator();
		}

		/// <summary>
		/// Hook used by new instances to load their XAML instead of retrieving
		/// it from the assembly.
		/// </summary>
		/// <param name="view">View.</param>
		public static void LoadXaml(object view)
		{
			loadXAML.Invoke(null, new object[] { view, currentXAML });
		}

		public Task<EvalResult> Eval(EvalRequest code, TaskScheduler mainScheduler, CancellationToken token)
		{
			var tcs = new TaskCompletionSource<EvalResult>();
			var r = new EvalResult();
			lock (mutex)
			{
				Task.Factory.StartNew(async () =>
				{
					try
					{
						r = await EvalOnMainThread(code, token);
						tcs.SetResult(r);
					}
					catch (Exception ex)
					{
						tcs.SetException(ex);
					}
				}, token, TaskCreationOptions.None, mainScheduler).Wait();
				return tcs.Task;
			}
		}

		async Task<EvalResult> EvalOnMainThread(EvalRequest code, CancellationToken token)
		{
			EvalResult evalResult = new EvalResult();

			var sw = new System.Diagnostics.Stopwatch();

			Log.Debug($"Evaluation request {code}");

			sw.Start();

			currentXAML = code.Xaml;
			if (!await evaluator.EvaluateExpression(code.NewTypeExpression,
													   code.NeedsRebuild ? code.Declarations : null,
													   evalResult))
			{
				// Try again recompiling just in case
				await evaluator.EvaluateExpression(code.NewTypeExpression,
													  code.Declarations,
													  evalResult);
			}

			if (evalResult.Result != null)
			{
				LoadXAML(evalResult.Result, code.Xaml, evalResult);
			}
			sw.Stop();

			Log.Debug($"Evaluation ended with result  {evalResult.Result}");

			evalResult.Duration = sw.Elapsed;
			return evalResult;
		}


		bool LoadXAML(object view, string xaml, EvalResult result)
		{
			Log.Information($"Loading XAML for type  {view}");
			try
			{
				loadXAML.Invoke(null, new object[] { view, xaml });
				Log.Information($"XAML loaded correctly for view {view}");
				return true;
			}
			catch (TargetInvocationException ex)
			{
				Log.Error($"Error loading XAML");
				result.Messages = new EvalMessage[] { new EvalMessage {
						MessageType = "error", Text = ex.ToString()}
				};
			}
			return false;
		}

		static void ResolveLoadMethod()
		{
			var asms = AppDomain.CurrentDomain.GetAssemblies();
			var xamlAssembly = Assembly.Load(new AssemblyName("Xamarin.Forms.Xaml"));
			var xamlLoader = xamlAssembly.GetType("Xamarin.Forms.Xaml.XamlLoader");
			loadXAML = xamlLoader.GetRuntimeMethod("Load", new[] { typeof(object), typeof(string) });
		}
	}
}

