using System;
using System.Threading.Tasks;

namespace XAMLator
{
	public interface ITcpCommunicator
	{
		event EventHandler<object> DataReceived;

		Task<bool> Send<T>(T obj);
	}
}