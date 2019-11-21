using RawPhysics;
using System.Collections;
using UnityEngine;
using MustHave;

public class Missile : MonoBehaviour
{
    private RawMissile rawMissile;

    public RawMissile RawMissile { get => rawMissile; set => rawMissile = value; }

    public Missile CreateInstance(Transform parent, RawMissile rawMissile)
    {
        Missile missile = Instantiate(this, rawMissile.Position, rawMissile.Rotation, parent);
        missile.transform.localScale  = transform.lossyScale;
        missile.RawMissile = rawMissile;
        missile.gameObject.SetActive(true);
        return missile;
    }

    //private void OnTriggerEnter2D(Collider2D collider)
    //{
    //}
}

public class RawMissile : RawBody2D
{
    private Spaceship spaceship;

    public RawMissile(Spaceship spaceship, Vector2 position, Vector3 eulerAngles, Bounds2 bounds) : base(position, eulerAngles, bounds)
    {
        this.spaceship = spaceship;
    }

    protected override void OnDestroyWithCollision(RawBody2D other)
    {
        if (other is RawAsteroid)
            AppManager.Instance.AddPlayerPoints(Const.PlayerPointsForAsteroid);
    }

    protected override GameObject GetGameObjectInstance(out SpriteRenderer spriteRenderer)
    {
        GameObject gameObject = spaceship.CreateMissileGameObject(this);
        spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        return gameObject;
    }

    protected override void RemoveGameObjectInstance()
    {
        spaceship.DestroyMissileGameObject(transform);
    }
}
