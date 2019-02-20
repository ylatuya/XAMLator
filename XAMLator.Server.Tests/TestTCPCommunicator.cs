using System;
using System.Threading.Tasks;

namespace XAMLator.Server.Tests
{
	public class TestTCPCommunicator : ITcpCommunicatorClient, ITcpCommunicatorServer
	{
		public event EventHandler<object> DataReceived;
		public event EventHandler ClientConnected;

		public int ClientsCount { get; private set; }

		public Task<bool> Connect(string ip, int port)
		{
			ClientsCount++;
			return Task.FromResult(true);
		}

		public void Disconnect()
		{
			ClientsCount--;
		}

		public Task<bool> Send<T>(T obj)
		{
			try
			{
				DataReceived?.Invoke(this, Serializer.DeserializeJson(Serializer.SerializeJson(obj)));
				return Task.FromResult(true);
			}
			catch (Exception ex)
			{
				Log.Exception(ex);
				return Task.FromResult(false);
			}
		}

		public Task<bool> StartListening(int serverPort)
		{
			return Task.FromResult(true);
		}

		public void StopListening()
		{
		}
	}
}
