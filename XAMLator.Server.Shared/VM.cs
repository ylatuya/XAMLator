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
		readonly object mutex = new object();
		MethodInfo loadXAML;
		IEvaluator evaluator;

		public VM()
		{
			ResolveLoadMethod();
			evaluator = new Evaluator();
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
			object result = null;
			bool hasResult = false;
			EvalResult evalResult = new EvalResult();

			var sw = new System.Diagnostics.Stopwatch();

			Log.Debug($"Evaluation request {code}");

			sw.Start();

			if (code.Xaml != null)
			{
				result = await LoadXAML(code.Xaml, code.XamlType, code.Declarations, evalResult);
				hasResult = result != null;
			}
			else
			{
				throw new NotSupportedException();
			}
			sw.Stop();

			Log.Debug($"Evaluation ended with result  {result}");

			evalResult.Duration = sw.Elapsed;
			evalResult.Result = result;
			evalResult.HasResult = hasResult;
			return evalResult;
		}


		async Task<object> LoadXAML(string xaml, string xamlType, string codeBehind, EvalResult result)
		{
			Log.Information($"Loading XAML for type  {xamlType}");
			if (!await evaluator.CreateTypeInstance(xamlType, result))
			{
				return null;
			}
			try
			{
				loadXAML.Invoke(null, new object[] { result.Result, xaml });
				Log.Information($"XAML loaded correctly for view {result.Result}");
				return result.Result;
			}
			catch (TargetInvocationException ex)
			{
				Log.Error($"Error loading XAML");
				result.Messages = new EvalMessage[] { new EvalMessage {
						MessageType = "error", Text = ex.ToString()}
				};
			}
			return null;
		}

		void ResolveLoadMethod()
		{
			var asms = AppDomain.CurrentDomain.GetAssemblies();
			var xamlAssembly = Assembly.Load(new AssemblyName("Xamarin.Forms.Xaml"));
			var xamlLoader = xamlAssembly.GetType("Xamarin.Forms.Xaml.XamlLoader");
			loadXAML = xamlLoader.GetRuntimeMethod("Load", new[] { typeof(object), typeof(string) });
		}
	}
}

