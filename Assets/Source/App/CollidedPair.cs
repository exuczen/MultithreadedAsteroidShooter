using UnityEngine;
using DC;

public struct CollidedPair
{
    public SpriteBody2D bodyA;
    public SpriteBody2D bodyB;

    public CollidedPair(SpriteBody2D bodyA, SpriteBody2D bodyB)
    {
        this.bodyA = bodyA;
        this.bodyB = bodyB;
    }

    public void Destroy()
    {
        Vector2 midPt = (bodyA.Position + bodyB.Position) / 2f;
        if (midPt.IsInCameraView(Camera.main, 0.1f, 0.1f))
        {
            bodyA.Explode(midPt);
        }
        bodyA.gameObject.SetActive(false);
        bodyB.gameObject.SetActive(false);
    }
}