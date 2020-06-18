using System.Collections;
using UnityEngine;

/// <summary>
/// Base class for weapon types
/// </summary>
public abstract class Weapon : MonoBehaviour {
	public abstract bool Shooting { get; }
	public abstract bool Reloading { get; }
	public abstract int Ammo { get; }
	public abstract int MaxAmmo { get; }
	public abstract float ReloadTime { get; }
	public abstract float FireRate { get; }

	public abstract void Reload();
	protected abstract IEnumerator DoReload();

    public abstract void Shoot(Vector3 direction);
    protected abstract IEnumerator DoShoot(Vector3 direction);

}
