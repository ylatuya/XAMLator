using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace XAMLator
{
	public class TcpCommunicatorServer : TcpCommunicator
	{
		int serverPort;
		TcpListener listener;

		public event EventHandler ClientConnected;

		public TcpCommunicatorServer(int serverPort)
		{
			this.serverPort = serverPort;
		}

		public int ClientsCount => clients.Count;

		public Task<bool> StartListening()
		{
			var taskCompletion = new TaskCompletionSource<bool>();
			Task.Factory.StartNew(() => Run(taskCompletion), TaskCreationOptions.LongRunning);
			return taskCompletion.Task;
		}

		async Task Run(TaskCompletionSource<bool> tcs)
		{
			try
			{
				listener = new TcpListener(IPAddress.Any, serverPort);
				listener.Start();
			}
			catch (Exception ex)
			{
				tcs.SetException(ex);
			}
			Log.Information($"Tcp server listening at port {serverPort}");
			tcs.SetResult(true);

			// Loop
			for (; ; )
			{
				var client = await listener.AcceptTcpClientAsync();
				var token = new CancellationTokenSource();
				Receive(client, token.Token);
				var guid = Guid.NewGuid();
				clients[guid] = new Tuple<TcpClient, CancellationTokenSource>(client, token);
				Log.Information("New client connection");
				ClientConnected?.Invoke(this, null);
			}
		}

		public void StopListening()
		{
			foreach (var client in clients)
			{
				client.Value.Item1.Close();
				client.Value.Item2.Cancel();
			}
			clients.Clear();
			listener.Stop();
		}
	}

	public class TcpCommunicatorClient : TcpCommunicator
	{
		TcpClient client;

		public async Task<bool> Connect(string ip, int port)
		{
			Disconnect();
			client = new TcpClient();
			CancellationTokenSource tokenSrc = new CancellationTokenSource();
			await client.ConnectAsync(ip, port);
			Receive(client, tokenSrc.Token);
			return true;
		}

		public void Disconnect()
		{
			client?.Close();
			client?.Dispose();
		}
	}

	public abstract class TcpCommunicator
	{
		string pendingmsg;
		protected ConcurrentDictionary<Guid, Tuple<TcpClient, CancellationTokenSource>> clients = new ConcurrentDictionary<Guid, Tuple<TcpClient, CancellationTokenSource>>();

		public event EventHandler<object> DataReceived;

		public async Task<bool> Send<T>(T obj)
		{
			string json = Serializer.SerializeJson(obj);
			json += '\0';
			var encoding = new UTF8Encoding(false);
			byte[] bytesToSend = encoding.GetBytes(json);
			foreach (var client in clients)
			{
				if (client.Value.Item1.Connected)
				{
					Log.Debug($"Sending to:{client.Key}");
					await client.Value.Item1.GetStream().WriteAsync(bytesToSend, 0, bytesToSend.Length);
				}
				else
				{
					Log.Debug($"Failed to send to:{client.Key}");
					client.Value.Item1.Close();
					clients.TryRemove(client.Key, out Tuple<TcpClient, CancellationTokenSource> removedClient);
					removedClient?.Item2.Cancel();
				}
			}
			//Improve return if errors
			return true;
		}

		protected void Receive(TcpClient client, CancellationToken cancellationToken)
		{
			byte[] bytes = new byte[1024];
			int bytesRead = 0;

			Log.Debug("Start receiving updates from ide");
			Task.Run(async () =>
			{
				// Loop to receive all the data sent by the client.
				bytesRead = await client.GetStream().ReadAsync(bytes, 0, bytes.Length, cancellationToken);

				while (bytesRead != 0)
				{
					cancellationToken.ThrowIfCancellationRequested();

					// Translate data bytes to a UTF8 string.
					string msg;
					msg = Encoding.UTF8.GetString(bytes, 0, bytesRead);

					// Process the data sent by the client.
					if (pendingmsg != null)
					{
						msg = pendingmsg + msg;
						pendingmsg = null;
					}
					int t = msg.LastIndexOf('\0');
					if (t == -1)
					{
						pendingmsg = msg;
						msg = null;
					}
					else if (t != msg.Length - 1)
					{
						pendingmsg = msg.Substring(t + 1, msg.Length - t - 1);
						msg = msg.Substring(0, t);
					}
					if (msg != null)
					{
						var msgs = msg.Split('\0');
						foreach (var ms in msgs)
						{
							if (!string.IsNullOrWhiteSpace(ms))
							{
								Log.Debug(String.Format("Received: {0}", msg));
								DataReceived?.Invoke(this, Serializer.DeserializeJson(ms));
							}
						}
					}
					//Receive more bytes
					bytesRead = await client.GetStream().ReadAsync(bytes, 0, bytes.Length, cancellationToken);
				}

				Log.Debug("Receive stopped, disconnected");
			}, cancellationToken);
		}
	}
}
