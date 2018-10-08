using System;
using System.Threading.Tasks;

namespace XAMLator.Server
{
	public interface IEvaluator
	{
		/// <summary>
		/// Evaluates an expression and code before the expression if requested.
		/// </summary>
		/// <returns>A new instance of the type.</returns>
		/// <param name="evalExpression">Type name.</param>
		/// <param name="code">The class code.</param>
		Task<bool> EvaluateExpression(string evalExpression, string code, EvalResult evalResult);
	}
}
