using System;
using System.Threading.Tasks;

namespace XAMLator
{
	public interface ITcpCommunicatorServer : ITcpCommunicator
	{
		event EventHandler ClientConnected;

		int ClientsCount { get; }

		Task<bool> StartListening(int serverPort);

		void StopListening();
	}
}