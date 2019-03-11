using UnityEngine;
using DC;
using RawPhysics;

public struct CollidedPair
{
    public RawBody2D bodyA;
    public RawBody2D bodyB;

    public CollidedPair(RawBody2D bodyA, RawBody2D bodyB)
    {
        this.bodyA = bodyA;
        this.bodyB = bodyB;
    }

    public void Destroy()
    {
        if (bodyA.Explosive && bodyB.Explosive)
        {
            Vector2 midPt = (bodyA.Position + bodyB.Position) / 2f;
            if (midPt.IsInCameraView(Camera.main, 0.1f, 0.1f))
            {
                bodyA.Explode(midPt);
            }
        }
        else if (bodyA.Explosive)
        {
            bodyA.Explode(bodyA.Position);
        }
        else if (bodyB.Explosive)
        {
            bodyB.Explode(bodyB.Position);
        }
        bodyA.RemoveGameObjectAfterCollision(bodyB);
        bodyB.RemoveGameObjectAfterCollision(bodyA);
    }
}