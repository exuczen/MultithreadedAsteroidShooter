using UnityEngine;
using DC;

[RequireComponent(typeof(SpriteRenderer))]
public class Asteroid : MonoBehaviour
{
    private AsteroidCreator creator;

    private RawAsteroid rawAsteroid;

    public RawAsteroid RawAsteroid { get => rawAsteroid; set { rawAsteroid = value; } }

    public Asteroid CreateInstance(AsteroidCreator creator, Transform parent)
    {
        Asteroid asteroid = Instantiate(this, parent, false);
        asteroid.gameObject.SetActive(false);
        asteroid.rawAsteroid = null;
        asteroid.creator = creator;
        return asteroid;
    }

    public Asteroid CreateInstance(AsteroidCreator creator, Transform parent, RawAsteroid rawAsteroid)
    {
        Asteroid asteroid = Instantiate(this, rawAsteroid.Position, rawAsteroid.Rotation, parent);
        asteroid.gameObject.SetActive(true);
        asteroid.rawAsteroid = rawAsteroid;
        asteroid.creator = creator;
        return asteroid;
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        //Debug.Log(GetType() + ".OnTriggerEnter: " + collider);
        creator.AddAsteroidToRespawn(rawAsteroid);
        rawAsteroid.RemoveGameObject();
        if (transform.position.IsInCameraView(Camera.main, 0.1f, 0.1f))
        {
            rawAsteroid.Explode(transform.position);
        }
    }
}
