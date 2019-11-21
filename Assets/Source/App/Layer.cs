using UnityEngine;

public struct Layer
{
    public static readonly int Default = LayerMask.NameToLayer("Default");
    public static readonly int Asteroid = LayerMask.NameToLayer("Asteroid");
    public static readonly int Spaceship = LayerMask.NameToLayer("Spaceship");
    public static readonly int Missile = LayerMask.NameToLayer("Missile");
}
