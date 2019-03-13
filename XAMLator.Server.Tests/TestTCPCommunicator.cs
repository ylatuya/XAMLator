using System;
using System.Threading.Tasks;

namespace XAMLator.Server.Tests
{
	public class TestTCPCommunicatorServer : ITcpCommunicatorServer
	{
		public event EventHandler<object> DataReceived;
		public event EventHandler ClientConnected;

		public int ClientsCount { get; private set; }

		public TestTCPCommunicatorClient Client { get; set; }

		public Task<bool> Connect(string ip, int port)
		{
			ClientsCount++;
			ClientConnected?.Invoke(this, EventArgs.Empty);
			return Task.FromResult(true);
		}

		public void Disconnect()
		{
			ClientsCount--;
		}

		public Task<bool> StartListening(int serverPort)
		{
			return Task.FromResult(true);
		}

		public void StopListening()
		{
		}

		public Task<bool> Send<T>(T obj)
		{
			try
			{
				Client.EmitDataReceived(obj);
				return Task.FromResult(true);
			}
			catch (Exception ex)
			{
				Log.Exception(ex);
				return Task.FromResult(false);
			}
		}

		public void EmitDataReceived<T>(T obj)
		{
			DataReceived?.Invoke(this, Serializer.DeserializeJson(Serializer.SerializeJson(obj)));
		}

	}

	public class TestTCPCommunicatorClient : ITcpCommunicatorClient
	{
		public event EventHandler<object> DataReceived;

		public TestTCPCommunicatorServer Server { get; set; }

		public Task<bool> Connect(string ip, int port)
		{
			Server.Connect(ip, port);
			return Task.FromResult(true);
		}

		public void Disconnect()
		{
		}

		public Task<bool> Send<T>(T obj)
		{
			try
			{
				Server.EmitDataReceived(obj);
				return Task.FromResult(true);
			}
			catch (Exception ex)
			{
				Log.Exception(ex);
				return Task.FromResult(false);
			}
		}

		public void EmitDataReceived<T>(T obj)
		{
			DataReceived?.Invoke(this, Serializer.DeserializeJson(Serializer.SerializeJson(obj)));
		}

	}
}
