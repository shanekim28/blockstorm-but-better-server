using System.Collections;
using UnityEngine;

public abstract class Weapon : MonoBehaviour {
	public bool shooting { get; set; }
	public bool reloading { get; set; }

	public float fireRate { get; set; }

	public abstract void Reload();
	protected abstract IEnumerator DoReload();

    public abstract void Shoot(Vector3 direction);
    protected abstract IEnumerator DoShoot(Vector3 direction);

}
