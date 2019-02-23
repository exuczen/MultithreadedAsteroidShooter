using UnityEngine;

public class SpaceshipExplosion : MonoBehaviour
{
    [SerializeField]
    private ParticleSystem particles;

    private void Awake()
    {
        particles = GetComponent<ParticleSystem>();
    }

    private void OnParticleSystemStopped()
    {
        //Debug.LogWarning(GetType() + ".OnParticleSystemStopped");
        AppManager.Instance.StartFailRoutine();
    }

    public void Play()
    {
        particles.Play();
    }
}
