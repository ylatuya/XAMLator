using System;
using System.Threading.Tasks;

namespace XAMLator.Server
{
	public interface IEvaluator
	{
		/// <summary>
		/// Creates a new instance of the requested type .
		/// </summary>
		/// <returns>A new instance of the requested type.</returns>
		/// <param name="typeName">Type name.</param>
		Task<bool> CreateTypeInstance(string typeName, EvalResult evalResult);

		/// <summary>
		/// Creates a new type instance of class from its code.
		/// </summary>
		/// <returns>A new instance of the type.</returns>
		/// <param name="typeName">Type name.</param>
		/// <param name="code">The class code.</param>
		Task<bool> CreateNewTypeInstance(string typeName, string code, EvalResult evalResult);
	}
}
