using UnityEngine;
using DC;

[RequireComponent(typeof(SpriteRenderer))]
public class Asteroid : SpriteBody2D
{
    private AsteroidCreator creator;

    public Asteroid CreateInstance(AsteroidCreator creator, Transform parent, Vector3 position)
    {
        Asteroid asteroid = CreateInstance<Asteroid>(parent, position, 0.65f);
        asteroid.creator = creator;
        return asteroid;
    }

    public void SetRandomPositionInBounds(Vector2 bottomLeft, Vector2 size)
    {
        int randomSignX = (Random.Range(0, 2) << 1) - 1;
        int randomSignY = (Random.Range(0, 2) << 1) - 1;
        Camera camera = Camera.main;
        Vector2 middlePos = bottomLeft + (size / 2f);
        Vector2 camHalfSize = new Vector2(camera.orthographicSize * Screen.width / Screen.height, camera.orthographicSize);
        float posX, posY;
        if (Random.Range(0, 2) == 0)
        {
            posX = middlePos.x + randomSignX * Random.Range(camHalfSize.x + bounds.extents.x, size.x / 2f - bounds.size.x);
            posY = middlePos.y + randomSignY * Random.Range(0f, size.y / 2f - bounds.size.y);
        }
        else
        {
            posY = middlePos.y + randomSignY * Random.Range(camHalfSize.y + bounds.extents.y, size.y / 2f - bounds.size.y);
            posX = middlePos.x + randomSignX * Random.Range(0f, size.x / 2f - bounds.size.x);
        }
        //float posX = middlePos.x + randomSignX * Random.Range(0f, camHalfSize.x);
        //float posY = middlePos.y + randomSignY * Random.Range(0f, camHalfSize.y);
        //float posX = bottomLeft.x + Random.Range(bounds.extents.x, size.x - bounds.size.x);
        //float posY = bottomLeft.y + Random.Range(bounds.extents.x, size.y - bounds.size.y);
        transform.position = Position = new Vector2(posX, posY);
    }

    public void SetRandomVelocities(float linearVelocityMin, float linearVelocityMax, float angularVelocityMin, float angularVelocityMax)
    {
        float angularVelocityNorm = Random.Range(-1f, 1f);
        float angularVelocity = Mathf.Sign(angularVelocityNorm) * Mathf.Lerp(angularVelocityMin, angularVelocityMax, Mathf.Abs(angularVelocityNorm));
        float linearVelocityMag = Mathf.Lerp(linearVelocityMin, linearVelocityMax, Random.Range(0f, 1f));
        float linearVelocityAngle = Random.Range(0f, 2 * Mathf.PI);
        Vector2 linearVelocity = new Vector2(linearVelocityMag * Mathf.Cos(linearVelocityAngle), linearVelocityMag * Mathf.Sin(linearVelocityAngle));
        //Vector2 linearVelocity = Vector2.zero;
        if (Const.Multithreading)
        {
            SetVelocities(linearVelocity, angularVelocity);
        }
        else
        {
            rigidBody.angularVelocity = angularVelocity;
            rigidBody.velocity = linearVelocity;
        }
    }

    public override void Respawn()
    {
        creator.RespawnAsteroid(this);
    }

    public override void Explode(Vector2 explosionPos)
    {
        creator.StartExplosion(explosionPos);
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        //Debug.Log(GetType() + ".OnTriggerEnter: " + collider);
        creator.AddAsteroidToRespawn(this);
        if (transform.position.IsInCameraView(Camera.main, 0.1f, 0.1f))
        {
            Explode(transform.position);
        }
    }
}
