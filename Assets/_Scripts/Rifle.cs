using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rifle : Weapon {
	public Transform muzzle;

	public int maxAmmo;
	[SerializeField]
	private int currentAmmo;

	[Tooltip("Fire rate in RPM")]
	public float fireRate = 600;
	public int bulletsPerShot = 1;
	private bool shooting = false;

	public float reloadTime = 2.667f;
	public float reloadDelay = 0.7f;
	private bool reloading;

	public override bool Shooting { get => shooting; }
	public override bool Reloading { get => reloading; }
	public override int Ammo { get => currentAmmo; }
	public override int MaxAmmo { get => maxAmmo; }
	public override float ReloadTime { get => reloadTime; }
	public override float FireRate { get => fireRate; }

	private void Awake() {
		currentAmmo = maxAmmo;
		Debug.Log("Set maximum ammo");
		Player.OnDie += ResetAmmo;
	}

	private void FixedUpdate() {
		// TODO: Play networked sound when firing or reloading

	}

	// TODO: Send ammo updates on death
	private void ResetAmmo() {
		currentAmmo = maxAmmo;
	}

	public override void Reload() {
		reloading = true;
		StartCoroutine(DoReload());
	}

	public override void Shoot(Vector3 direction) {
		StartCoroutine(DoShoot(direction));
	}

	protected override IEnumerator DoReload() {
		ServerSend.PlayerReloadAnimation(GetComponent<Player>(), this);
		yield return new WaitForSeconds(reloadTime);
		currentAmmo = maxAmmo;

		reloading = false;
	}

	protected override IEnumerator DoShoot(Vector3 direction) {
		if (!reloading && !shooting && currentAmmo > 0) {
			shooting = true;
			currentAmmo--;

			if (Physics.Raycast(muzzle.position, direction, out RaycastHit hit, 25f)) {

				if (hit.collider.CompareTag("Player")) {
					Player playerHit = hit.collider.GetComponentInParent<Player>();

					if (playerHit.id != GetComponentInParent<Player>().id) {
						playerHit.TakeDamage(10f);

					}

				}
			}

			ServerSend.PlayerShootAnimation(GetComponent<Player>(), this);

			yield return new WaitForSeconds(1 / (fireRate / 60));
			shooting = false;
		}
	}
}
