using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class SpaceshipExplosion : MonoBehaviour
{
    public SpaceshipExplosion CreateInstance(Vector3 position)
    {
        return Instantiate(this, position, Quaternion.identity);
    }

    private void OnParticleSystemStopped()
    {
        AppManager.Instance.StartFailRoutine();
        Destroy(gameObject);
    }
}
