using System.Collections;
using UnityEngine;

public class Player : MonoBehaviour {
	public int id;
	public string username;

	public Rigidbody rb;
	public float gravity = -9.81f;
	public float moveSpeed = 5f;
	public float jumpSpeed = 5f;

	public Transform shootOrigin;
	public float health;
	public float maxHealth = 100f;

	private bool[] inputs;
	private float yVelocity = 0;
	public void Initialize(int id, string username) {
		this.id = id;
		this.username = username;

		health = maxHealth;

		inputs = new bool[5];
	}

	private void Start() {
	}

	public void FixedUpdate() {
		if (health <= 0) {
			return;
		}

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

	public void Shoot(Vector3 direction) {
		if (Physics.Raycast(shootOrigin.position, direction, out RaycastHit hit, 25f)) {

			if (hit.collider.CompareTag("Player")) {
				hit.collider.GetComponentInParent<Player>().TakeDamage(50f);
			}
		}
	}

	public void TakeDamage(float damage) {
		Debug.LogError($"Player {id} took damage");
		if (health <= 0f) {
			return;
		}

		health -= damage;

		// TODO: Add more death stuff
		// NOTE: Some parts of player death are handled client-side, like viewmodel disappearing and resetting health after respawn
		if (health <= 0f) {
			health = 0f;

			// TODO: Disable rigidbody
			GetComponent<Rigidbody>().isKinematic = true;
			transform.position = new Vector3(0, 10f, 0);
			ServerSend.PlayerPosition(this);
			StartCoroutine(Respawn());
		}

		ServerSend.PlayerHealth(this);
	}

	private IEnumerator Respawn() {
		yield return new WaitForSeconds(5f);

		health = maxHealth;

		// TODO: Re-enable rigidbody
		GetComponent<Rigidbody>().isKinematic = false;
		ServerSend.PlayerRespawned(this);
	}
}
