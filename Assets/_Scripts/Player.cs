using System.Collections;
using UnityEngine;

// TODO: Change jump to TCP (optional)
public class Player : MonoBehaviour {
	public int id;
	public string username;

	public Rigidbody rb;
	public float gravity = -9.81f;
	public float moveSpeed = 15f;
	public float jumpSpeed = 15f;

	[SerializeField]
	float minWallrunSpeed;

	private bool grounded;

	[SerializeField] float wallRunDist = 5;
	public float wallrunCooldown;
	[SerializeField]
	private float wallrunTimer = 0f;

	internal Quaternion rotation;

	public Transform shootOrigin;
	public float health;
	public float maxHealth = 100f;

	private bool[] inputs;

	public PlayerState currentState = PlayerState.Idle;
	private PlayerState lastState;
	private Vector3 vectorAlongWall;

	private void Start() {
		rotation = Quaternion.identity;
	}

	public enum PlayerState {
		Idle = 1,
		Walking, Falling, Airwalking, Jumping,
		WallrunLeft, WallrunRight, 
		Grappling
	}

	public void Initialize(int id, string username) {
		this.id = id;
		this.username = username;

		health = maxHealth;

		inputs = new bool[5];
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

		if (wallrunTimer > 0) {
			wallrunTimer -= Time.deltaTime;
		} else {
			wallrunTimer = 0;
		}

		AssignPlayerState();
		Move(inputDirection);
	}

	private void Move(Vector2 inputDirection) {
		Vector3 moveDirection = transform.right * inputDirection.x + transform.forward * inputDirection.y;
		moveDirection *= moveSpeed;

		if (grounded) {
			// Movement
			rb.velocity = moveDirection + Vector3.up * rb.velocity.y;

			if (inputs[4]) {
				// Jump
				rb.AddForce(Vector3.up * jumpSpeed, ForceMode.VelocityChange);
			}
		} else {
			if (currentState != PlayerState.WallrunLeft && currentState != PlayerState.WallrunRight) {
				// Airwalking
				rb.AddForce(moveDirection, ForceMode.Acceleration);
			}
		}

		CheckWallrun();

		// TODO: Send animation parameters
		ServerSend.PlayerPosition(this);
		ServerSend.PlayerRotation(this);
		ServerSend.PlayerWallrun(this, vectorAlongWall);

	}

	/// <summary>
	/// Checks if the player can wallrun
	/// </summary>
	private void CheckWallrun() {
		// Return if the player can't wallrun
		if (wallrunTimer != 0 || grounded || Vector3.Dot(rb.velocity, transform.forward) < minWallrunSpeed) return;

		bool wallToLeft = false;
		bool wallToRight = false;

		Vector3 forwardAndRight = (transform.forward + transform.right).normalized;
		Vector3 forwardAndLeft = (transform.forward - transform.right).normalized;

		// Forward and right multiplies by sqrt 2 because 45-45-90 triangle
		if ((Physics.Raycast(transform.position, -transform.right, out RaycastHit leftHit, wallRunDist) || Physics.Raycast(transform.position, forwardAndLeft, out leftHit, wallRunDist * Mathf.Sqrt(2))) 
			&& Vector3.Dot(leftHit.normal, Vector3.up) == 0) {
			wallToLeft = true;
		}

		if ((Physics.Raycast(transform.position, transform.right, out RaycastHit rightHit, wallRunDist) || Physics.Raycast(transform.position, forwardAndRight, out rightHit, wallRunDist * Mathf.Sqrt(2))) 
			&& Vector3.Dot(rightHit.normal, Vector3.up) == 0) {
			wallToRight = true;
		}

		// If we have exactly one wallrunning option,
		// find out which one it is and wallrun on that side.
		if (wallToLeft ^ wallToRight) {	
			if (wallToLeft) {
				currentState = PlayerState.WallrunLeft;
				WallRun(-Vector3.Cross(Vector3.up, leftHit.normal) - transform.right * 0.2f);
			} else {
				currentState = PlayerState.WallrunRight;
				WallRun(Vector3.Cross(Vector3.up, rightHit.normal) + transform.right * 0.2f);
			}
			// If we have two wallrunning options,
			// wallrun on the side that's closer
		} else if (wallToLeft && wallToRight) {
			if (leftHit.distance < rightHit.distance) {
				currentState = PlayerState.WallrunLeft;
				WallRun(-Vector3.Cross(Vector3.up, leftHit.normal) - transform.right * 0.2f);

			} else {
				currentState = PlayerState.WallrunRight;
				WallRun(Vector3.Cross(Vector3.up, rightHit.normal) + transform.right * 0.2f);

			}
		}

		// If the previous state changed, reset the cooldown
		if (currentState != lastState) {
			if (lastState == PlayerState.WallrunLeft || lastState == PlayerState.WallrunRight) {
				wallrunTimer = wallrunCooldown;
			}
		}
	}

	/// <summary>
	/// Sets the velocity to begin wallrunning
	/// </summary>
	/// <param name="vectorAlongWall">The vector along the wall</param>
	private void WallRun(Vector3 vectorAlongWall) {
		this.vectorAlongWall = vectorAlongWall;
		rb.velocity = vectorAlongWall * moveSpeed * 1.5f;

		// Jump while wallrunning
		if (inputs[4]) {
			if (currentState == PlayerState.WallrunLeft) {
				rb.AddForce((transform.up + transform.right).normalized * jumpSpeed * 2, ForceMode.VelocityChange);
				wallrunTimer = wallrunCooldown;
			} else if (currentState == PlayerState.WallrunRight) {
				rb.AddForce((transform.up - transform.right).normalized * jumpSpeed * 2, ForceMode.VelocityChange);
				wallrunTimer = wallrunCooldown;
			}
		}
	}

	/// <summary>
	/// Assigns player state based on velocity
	/// </summary>
	private void AssignPlayerState() {
		lastState = currentState;

		if (grounded) {
			currentState = PlayerState.Idle;
		} else {
			if (rb.velocity.y > 0) {
				currentState = PlayerState.Jumping;
			} else {
				currentState = PlayerState.Falling;
			}
		}

		// If moving
		if (Mathf.Round(rb.velocity.x) != 0f || Mathf.Round(rb.velocity.z) != 0f) {
			if (grounded) {
				currentState = PlayerState.Walking;
			} else {
				if (inputs[0] || inputs[1] || inputs[2] || inputs[3]) {

					currentState = PlayerState.Airwalking;
				}
			}
		}
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

	private void OnTriggerEnter(Collider other) {
		grounded = true;
	}

	private void OnTriggerExit(Collider other) {
		grounded = false;
	}
}
