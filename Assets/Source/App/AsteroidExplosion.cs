using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class AsteroidExplosion : MonoBehaviour
{
    private ParticleSystem particles = default;

    private void Awake()
    {
        particles = GetComponent<ParticleSystem>();
    }

    public void Play()
    {
        particles.Play();
    }
}
