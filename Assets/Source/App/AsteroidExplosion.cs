using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class AsteroidExplosion : MonoBehaviour
{
    private ParticleSystem particles;

    private void Awake()
    {
        particles = GetComponent<ParticleSystem>();
        //ParticleSystem.MainModule mainModule = particles.main;
        //mainModule.stopAction = ParticleSystemStopAction.Destroy;
        //particles.main = mainModule;
    }

    public void Play()
    {
        particles.Play();
    }
}
