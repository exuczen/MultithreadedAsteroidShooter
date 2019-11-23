using UnityEngine;

namespace RawPhysics
{
    public class RawCirclesCollision2D : RawCollision2D<RawCircleCollider2D, RawCircleCollider2D>
    {
        public static float basicDiameterSquared;

        public override bool Overlap(RawCircleCollider2D circle1, RawCircleCollider2D circle2, Vector2 bodiesRay)
        {
            float radiusSum = circle1.Radius + circle1.Radius;
            return bodiesRay.x * bodiesRay.x + bodiesRay.y * bodiesRay.y <= radiusSum * radiusSum;
        }

        public bool BasicRadiusOverlap(RawCircleCollider2D circle1, RawCircleCollider2D circle2, Vector2 bodiesRay)
        {
            return bodiesRay.x * bodiesRay.x + bodiesRay.y * bodiesRay.y <= basicDiameterSquared;
        }
    }
}
