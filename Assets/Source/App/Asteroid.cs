using UnityEngine;
using MustHave;
using RawPhysics;

[RequireComponent(typeof(SpriteRenderer))]
public class Asteroid : MonoBehaviour
{
    [SerializeField] private float radius = default;

    private RawAsteroid rawAsteroid = default;

    public RawAsteroid RawAsteroid { get => rawAsteroid; set { rawAsteroid = value; } }
    public float Radius { get => radius; }

    public Asteroid CreateInstance(Transform parent)
    {
        Asteroid asteroid = Instantiate(this, parent, false);
        asteroid.gameObject.SetActive(false);
        asteroid.rawAsteroid = null;
        return asteroid;
    }

    public Asteroid CreateInstance(Transform parent, RawAsteroid rawAsteroid)
    {
        Asteroid asteroid = Instantiate(this, rawAsteroid.Position, rawAsteroid.Rotation, parent);
        asteroid.gameObject.SetActive(true);
        asteroid.rawAsteroid = rawAsteroid;
        return asteroid;
    }
}

public class RawAsteroid : RawBody2D
{
    private readonly AsteroidCreator creator = default;

    public RawAsteroid(AsteroidCreator creator, Vector2 position, Vector3 eulerAngles, Bounds2 bounds, float radius)
        : base(position, eulerAngles, bounds)
    {
        this.creator = creator;
        respawnable = true;
        explosive = true;
        SetCollider(new RawCircleCollider2D(radius));
    }

    public void SetRandomPositionInBounds(Bounds2 rectBounds, Bounds2 camBounds)
    {
        int randomSignX = (Random.Range(0, 2) << 1) - 1;
        int randomSignY = (Random.Range(0, 2) << 1) - 1;
        Vector2 middlePos = rectBounds.Center;
        float posX, posY;
        if (Random.Range(0, 2) == 0)
        {
            posX = middlePos.x + randomSignX * Random.Range(camBounds.Extents.x + bounds.Extents.x, rectBounds.Extents.x - bounds.Size.x);
            posY = middlePos.y + randomSignY * Random.Range(0f, rectBounds.Extents.y - bounds.Size.y);
        }
        else
        {
            posY = middlePos.y + randomSignY * Random.Range(camBounds.Extents.y + bounds.Extents.y, rectBounds.Extents.y - bounds.Size.y);
            posX = middlePos.x + randomSignX * Random.Range(0f, rectBounds.Extents.x - bounds.Size.x);
        }
        Position = new Vector2(posX, posY);
    }

    public void SetRandomVelocities(float linearVelocityMin, float linearVelocityMax, float angularVelocityMin, float angularVelocityMax)
    {
        float angularVelocityNorm = Random.Range(-1f, 1f);
        float angularVelocity = Mathf.Sign(angularVelocityNorm) * Mathf.Lerp(angularVelocityMin, angularVelocityMax, Mathf.Abs(angularVelocityNorm));
        float linearVelocityMag = Mathf.Lerp(linearVelocityMin, linearVelocityMax, Random.Range(0f, 1f));
        float linearVelocityAngle = Random.Range(0f, 2 * Mathf.PI);
        Vector2 linearVelocity = new Vector2(linearVelocityMag * Mathf.Cos(linearVelocityAngle), linearVelocityMag * Mathf.Sin(linearVelocityAngle));
        SetVelocities(linearVelocity, angularVelocity);
    }

    public override void Respawn()
    {
        creator.RespawnAsteroid(this);
    }

    public override void Explode(Vector2 explosionPos)
    {
        creator.StartExplosion(explosionPos);
    }

    protected override GameObject GetGameObjectInstance(out SpriteRenderer spriteRenderer)
    {
        GameObject gameObject = creator.PickAsteroidGameObjectFromPool(this);
        spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        return gameObject;
    }

    protected override void RemoveGameObjectInstance()
    {
        creator.ReturnAsteroidGameObjectToPool(transform);
    }
}