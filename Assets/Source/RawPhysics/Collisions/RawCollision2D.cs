using UnityEngine;

namespace RawPhysics
{
    public abstract class RawCollision2D<T1, T2> where T1 : RawCollider2D where T2 : RawCollider2D
    {
        public bool Overlap(T1 collider1, T2 collider2)
        {
            return Overlap(collider1, collider2, collider2.Body.Position - collider1.Body.Position);
        }

        public abstract bool Overlap(T1 collider1, T2 collider2, Vector2 bodiesRay);
    }
}
