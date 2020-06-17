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
	private new bool shooting = false;

	public float reloadTime = 2.667f;
	public float reloadDelay = 0.7f;
	private new bool reloading;

	private void Awake() {
		currentAmmo = maxAmmo;
		Debug.Log("Set maximum ammo");
	}

	private void FixedUpdate() {
		// TODO: Fix animation double shooting on client-side
		// Send a single TCP packet each time the client fires to sync
		// TODO: Play networked sound when firing or reloading
		base.reloading = reloading;
		base.shooting = shooting;
		base.fireRate = fireRate;
	}

	public override void Reload() {
		reloading = true;
		StartCoroutine(DoReload());
	}

	public override void Shoot(Vector3 direction) {
		StartCoroutine(DoShoot(direction));
	}

	protected override IEnumerator DoReload() {
		ServerSend.PlayerReloadAnimation(GetComponent<Player>());
		yield return new WaitForSeconds(reloadTime - 0.05f);
		currentAmmo = maxAmmo;
		reloading = false;
	}

	protected override IEnumerator DoShoot(Vector3 direction) {
		if (!reloading && !shooting && currentAmmo > 0) {
			shooting = true;
			currentAmmo--;

			if (Physics.Raycast(muzzle.position, direction, out RaycastHit hit, 25f)) {

				if (hit.collider.CompareTag("Player")) {
					hit.collider.GetComponentInParent<Player>().TakeDamage(10f);
				}
			}

			ServerSend.PlayerShootAnimation(GetComponent<Player>());

			yield return new WaitForSeconds(1 / (fireRate / 60));
			shooting = false;
		}
	}
}
