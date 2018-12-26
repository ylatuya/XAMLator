using System.Threading.Tasks;

namespace XAMLator
{
	public interface ITcpCommunicatorClient : ITcpCommunicator
	{
		Task<bool> Connect(string ip, int port);

		void Disconnect();
	}
}