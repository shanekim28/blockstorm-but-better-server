using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerSend {
	#region Packets
	public static void Welcome(int toClient, string msg) {
		using (Packet packet = new Packet((int) ServerPackets.welcome)) {
			packet.Write(msg);
			packet.Write(toClient);

			SendTCPData(toClient, packet);
		}
	}

	public static void SpawnPlayer(int toClient, Player player) {
		using (Packet packet = new Packet((int) ServerPackets.spawnPlayer)) {
			packet.Write(player.id);
			packet.Write(player.username);
			packet.Write(player.transform.position);
			packet.Write(player.transform.rotation);

			SendTCPData(toClient, packet);
		}
	}

	public static void PlayerPosition(Player player) {
		using (Packet packet = new Packet((int) ServerPackets.playerPosition)) {
			packet.Write(player.id);
			packet.Write(player.transform.position);

			SendUDPDataToAll(packet);
		}
	}

	public static void PlayerRotation(Player player) {
		using (Packet packet = new Packet((int) ServerPackets.playerRotation)) {
			packet.Write(player.id);
			packet.Write(player.transform.rotation);

			SendUDPDataToAll(player.id, packet);
		}
	}

	public static void PlayerDisconnect(int playerId) {
		using (Packet packet = new Packet((int) ServerPackets.playerDisconnected)) {
			packet.Write(playerId);

			SendTCPDataToAll(packet);
		}
	}

	public static void PlayerHealth(Player player) {
		using (Packet packet = new Packet((int) ServerPackets.playerHealth)) {
			packet.Write(player.id);
			packet.Write(player.health);

			SendTCPDataToAll(packet);
		}
	}

	public static void PlayerRespawned(Player player) {
		using (Packet packet = new Packet((int) ServerPackets.playerRespawned)) {
			packet.Write(player.id);

			SendTCPDataToAll(packet);
		}
	}

	#endregion

	private static void SendUDPData(int toClient, Packet packet) {
		packet.WriteLength();
		Server.clients[toClient].udp.SendData(packet);
	}

	private static void SendUDPDataToAll(Packet packet) {
		packet.WriteLength();
		for (int i = 1; i <= Server.MaxPlayers; i++) {
			Server.clients[i].udp.SendData(packet);
		}
	}

	private static void SendUDPDataToAll(int exceptClient, Packet packet) {
		packet.WriteLength();
		for (int i = 1; i <= Server.MaxPlayers; i++) {
			if (i != exceptClient) {
				Server.clients[i].udp.SendData(packet);
			}
		}
	}

	private static void SendTCPData(int toClient, Packet packet) {
		packet.WriteLength();
		Server.clients[toClient].tcp.SendData(packet);
	}

	private static void SendTCPDataToAll(Packet packet) {
		packet.WriteLength();
		for (int i = 1; i <= Server.MaxPlayers; i++) {
			Server.clients[i].tcp.SendData(packet);
		}
	}

	private static void SendTCPDataToAll(int exceptClient, Packet packet) {
		packet.WriteLength();
		for (int i = 1; i <= Server.MaxPlayers; i++) {
			if (i != exceptClient) {
				Server.clients[i].tcp.SendData(packet);
			}
		}
	}
}