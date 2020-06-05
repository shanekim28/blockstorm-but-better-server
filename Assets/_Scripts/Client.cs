using System;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

/// <summary>
/// Stores client information
/// </summary>
public class Client {
	public static int dataBufferSize = 4096;

	public int id;
	public TCP tcp;
	public UDP udp;
	public Player player;
	public Client(int clientId) {
		id = clientId;
		tcp = new TCP(id);
		udp = new UDP(id);
	}

	public class UDP {
		public IPEndPoint endPoint;

		private int id;

		public UDP(int id) {
			this.id = id;
		}

		public void Connect(IPEndPoint endPoint) {
			this.endPoint = endPoint;
		}

		public void SendData(Packet packet) {
			Server.SendUDPData(endPoint, packet);
		}

		public void HandleData(Packet packetData) {
			int packetLength = packetData.ReadInt();
			byte[] packetBytes = packetData.ReadBytes(packetLength);

			ThreadManager.ExecuteOnMainThread(() => {
				using (Packet packet = new Packet(packetBytes)) {
					int packetId = packet.ReadInt();
					Server.packetHandlers[packetId](id, packet);
				}
			});
		}

		public void Disconnect() {
			endPoint = null;
		}
	}

	public class TCP {
		public TcpClient socket;
		private readonly int id;
		private NetworkStream stream;
		private Packet receivedData;
		private byte[] receiveBuffer;

		public TCP(int id) {
			this.id = id;
		}

		public void Connect(TcpClient socket) {
			this.socket = socket;
			this.socket.ReceiveBufferSize = dataBufferSize;
			this.socket.SendBufferSize = dataBufferSize;

			stream = socket.GetStream();

			receivedData = new Packet();
			receiveBuffer = new byte[dataBufferSize];

			stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);

			ServerSend.Welcome(id, "Welcome to the server!");
		}

		public void SendData(Packet packet) {
			try {
				if (socket != null) {
					stream.BeginWrite(packet.ToArray(), 0, packet.Length(), null, null);
				}
			} catch (Exception e) {
				Debug.Log($"Error sending data to player {id} via TCP: {e}");
			}
		}

		private void ReceiveCallback(IAsyncResult result) {
			try {
				int byteLength = stream.EndRead(result);
				if (byteLength <= 0) {
					Server.clients[id].Disconnect();
					return;
				}

				byte[] data = new byte[byteLength];
				Array.Copy(receiveBuffer, data, byteLength);

				receivedData.Reset(HandleData(data));
				stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);
			} catch (Exception e) {
				Debug.Log("Error receiving TCP data: " + e.ToString());
				Server.clients[id].Disconnect();

			}
		}

		private bool HandleData(byte[] data) {
			int packetLength = 0;

			receivedData.SetBytes(data);
			if (receivedData.UnreadLength() >= 4) {
				packetLength = receivedData.ReadInt();
				if (packetLength <= 0) {
					return true;
				}
			}

			while (packetLength > 0 && packetLength <= receivedData.UnreadLength()) {
				byte[] packetBytes = receivedData.ReadBytes(packetLength);
				ThreadManager.ExecuteOnMainThread(() => {
					using (Packet packet = new Packet(packetBytes)) {
						int packetId = packet.ReadInt();
						Server.packetHandlers[packetId](id, packet);
					}
				});

				packetLength = 0;
				if (receivedData.UnreadLength() >= 4) {
					packetLength = receivedData.ReadInt();
					if (packetLength <= 0) {
						return true;
					}
				}
			}

			if (packetLength <= 1) {
				return true;
			}

			return false;
		}

		public void Disconnect() {
			socket.Close();
			stream = null;
			receivedData = null;
			socket = null;
		}
	}


	public void SendIntoGame(string playerName) {
		player = NetworkManager.instance.InstantiatePlayer();
		player.Initialize(id, playerName);

		// Send information from all other clients to newly connected client
		foreach (Client client in Server.clients.Values) {
			if (client.player != null) {
				if (client.id != id) {
					ServerSend.SpawnPlayer(id, client.player);
				}
			}
		}

		// Send new player's information to all players
		foreach (Client client in Server.clients.Values) {
			if (client.player != null) {
				ServerSend.SpawnPlayer(client.id, client.player);
			}
		}
	}
	public void Disconnect() {
		Debug.Log($"{tcp.socket.Client.RemoteEndPoint} has disconnected.");

		UnityEngine.Object.Destroy(player.gameObject);
		player = null;
		tcp.Disconnect();
		udp.Disconnect();
	}
}