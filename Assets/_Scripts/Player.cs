using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {
	public int id;
	public string username;

	public Rigidbody rb;
	public float gravity = -9.81f;
	public float moveSpeed = 5f;
	public float jumpSpeed = 5f;

	private bool[] inputs;
	private float yVelocity = 0;
	public void Initialize(int id, string username) {
		this.id = id;
		this.username = username;

		inputs = new bool[5];
	}

	private void Start() {
	}

	public void FixedUpdate() {
		Vector2 inputDirection = Vector2.zero;

		if (inputs[0]) {
			inputDirection.y += 1;
		}
		if (inputs[1]) {
			inputDirection.x -= 1;
		}
		if (inputs[2]) {
			inputDirection.y -= 1;
		}
		if (inputs[3]) {
			inputDirection.x += 1;
		}

		Move(inputDirection);
	}

	private void Move(Vector2 inputDirection) {
		Vector3 moveDirection = transform.right * inputDirection.x + transform.forward * inputDirection.y;
		moveDirection *= moveSpeed;

		// TODO: Check grounded and apply proper y velocities
		if (inputs[4]) {
			rb.AddForce(Vector3.up * jumpSpeed, ForceMode.VelocityChange);
		}

		rb.velocity = moveDirection + new Vector3(0, rb.velocity.y, 0);

		ServerSend.PlayerPosition(this);
		ServerSend.PlayerRotation(this);
	}

	public void SetInput(bool[] inputs, Quaternion rotation) {
		this.inputs = inputs;
		transform.rotation = rotation;
	}
}
