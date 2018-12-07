using System.Threading.Tasks;

namespace XAMLator.Server
{
	public interface IEvaluator
	{
		/// <summary>
		/// Evaluates an expression and code before the expression if requested.
		/// </summary>
		/// <returns>True if succeeded.</returns>
		/// <param name="code">The class code.</param>
		/// <param name="result">Eval result.</param>
		Task<bool> EvaluateCode(string code, EvalResult result);

		/// <summary>
		/// Check if evaluation is supported. This can fail in iOS real devices
		/// or in iOS device emulators if --enable-repl is not passed as an option
		/// to the linked
		/// </summary>
		/// <returns><c>true</c>, if evaluation is supported, <c>false</c> otherwise.</returns>
		bool IsEvaluationSupported { get; }
	}
}
