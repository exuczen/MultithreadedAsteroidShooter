using MustHave.Utilities;
using System.Collections.Generic;
using UnityEngine;

namespace RawPhysics
{
    public class RawPhysics2D
    {
        public static readonly List<RawColliderShape2D> ColliderShapes = EnumUtils.GetList<RawColliderShape2D>();

        private static RawBoxCircleCollision2D boxCircleCollision = new RawBoxCircleCollision2D();

        private static RawTriangleCircleCollision2D triangleCircleCollision = new RawTriangleCircleCollision2D();

        private static RawCirclesCollision2D circlesCollision = new RawCirclesCollision2D();

        static RawPhysics2D() { }

        public static bool BoxCircleOverlap(RawBoxCollider2D collider1, RawCircleCollider2D collider2)
        {
            return boxCircleCollision.Overlap(collider1, collider2);
        }

        public static bool BoxCircleOverlap(RawBoxCollider2D collider1, RawCircleCollider2D collider2, Vector2 bodiesRay)
        {
            return boxCircleCollision.Overlap(collider1, collider2, bodiesRay);
        }

        public static bool TriangleCircleOverlap(RawTriangleCollider2D collider1, RawCircleCollider2D collider2)
        {
            return triangleCircleCollision.Overlap(collider1, collider2);
        }

        public static bool TriangleCircleOverlap(RawTriangleCollider2D collider1, RawCircleCollider2D collider2, Vector2 bodiesRay)
        {
            return triangleCircleCollision.Overlap(collider1, collider2, bodiesRay);
        }

        public static bool CirclesOverlap(RawCircleCollider2D collider1, RawCircleCollider2D collider2)
        {
            return circlesCollision.Overlap(collider1, collider2);
        }

        public static bool CirclesOverlap(RawCircleCollider2D collider1, RawCircleCollider2D collider2, Vector2 bodiesRay)
        {
            return circlesCollision.Overlap(collider1, collider2, bodiesRay);
        }

        public static bool CirclesWithBasicRadiusOverlap(RawCircleCollider2D collider1, RawCircleCollider2D collider2, Vector2 bodiesRay)
        {
            return circlesCollision.BasicRadiusOverlap(collider1, collider2, bodiesRay);
        }

        public static bool CirclesOverlapInt(RawCircleCollider2D collider1, RawCircleCollider2D collider2, Vector2Int bodiesRayInt)
        {
            return circlesCollision.OverlapInt(collider1, collider2, bodiesRayInt);
        }

    } 
}
