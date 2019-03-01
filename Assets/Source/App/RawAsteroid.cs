using UnityEngine;

public class RawAsteroid : RawBody2D
{
    private AsteroidCreator creator;

    public RawAsteroid(AsteroidCreator creator, Vector2 position, Vector3 eulerAngles, Bounds2 bounds, float radius) 
        : base(position, eulerAngles, bounds, radius)
    {
        this.creator = creator;
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
        //float posX = middlePos.x + randomSignX * Random.Range(0f, camHalfSize.x);
        //float posY = middlePos.y + randomSignY * Random.Range(0f, camHalfSize.y);
        //float posX = bottomLeft.x + Random.Range(bounds.extents.x, size.x - bounds.size.x);
        //float posY = bottomLeft.y + Random.Range(bounds.extents.x, size.y - bounds.size.y);
        Position = new Vector2(posX, posY);
    }

    public void SetRandomVelocities(float linearVelocityMin, float linearVelocityMax, float angularVelocityMin, float angularVelocityMax)
    {
        float angularVelocityNorm = Random.Range(-1f, 1f);
        float angularVelocity = Mathf.Sign(angularVelocityNorm) * Mathf.Lerp(angularVelocityMin, angularVelocityMax, Mathf.Abs(angularVelocityNorm));
        float linearVelocityMag = Mathf.Lerp(linearVelocityMin, linearVelocityMax, Random.Range(0f, 1f));
        float linearVelocityAngle = Random.Range(0f, 2 * Mathf.PI);
        Vector2 linearVelocity = new Vector2(linearVelocityMag * Mathf.Cos(linearVelocityAngle), linearVelocityMag * Mathf.Sin(linearVelocityAngle));
        //Vector2 linearVelocity = Vector2.zero;
        SetVelocities(linearVelocity, angularVelocity);
        //rigidBody.angularVelocity = angularVelocity;
        //rigidBody.velocity = linearVelocity;
    }

    public override void Respawn()
    {
        creator.RespawnAsteroid(this);
    }

    public override void Explode(Vector2 explosionPos)
    {
        creator.StartExplosion(explosionPos);
    }

    protected override GameObject GetGameObjectInstance()
    {
        return creator.PickAsteroidGameObjectFromPool(this);
    }

    protected override void RemoveGameObjectInstance()
    {
        creator.ReturnAsteroidGameObjectToPool(transform);
    }
}

