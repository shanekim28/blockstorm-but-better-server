﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerHandle {
	public static void WelcomeReceived(int fromClient, Packet packet) {
		int clientIdcheck = packet.ReadInt();
		string username = packet.ReadString();

		Debug.Log($"{Server.clients[fromClient].tcp.socket.Client.RemoteEndPoint} connected successfully and is now player {fromClient}.");
		if (fromClient != clientIdcheck) {
			// Should never run
			Debug.Log($"Player '{username}' (ID: {fromClient}) has assumed the wrong ID ({clientIdcheck})");
		}

		Server.clients[fromClient].SendIntoGame(username);
	}

	public static void PlayerMovement(int fromClient, Packet packet) {
		bool[] inputs = new bool[packet.ReadInt()];

		for (int i = 0; i < inputs.Length; i++) {
			inputs[i] = packet.ReadBool();
		}

		Quaternion rotation = packet.ReadQuaternion();

		Server.clients[fromClient].player.SetInput(inputs, rotation);
	}

	public static void PlayerShoot(int fromClient, Packet packet) {
		Vector3 shootDirection = packet.ReadVector3();
		Weapon weapon = Server.clients[fromClient].player.GetComponent<Weapon>();

		weapon.Shoot(shootDirection);
	}

	public static void PlayerReload(int fromClient, Packet packet) {
		Debug.LogError($"Player {fromClient} reloaded");
		Weapon weapon = Server.clients[fromClient].player.GetComponent<Weapon>();

		weapon.Reload();
	}
}
