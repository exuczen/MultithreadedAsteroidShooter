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

    public class RawTriangleCircleCollision2D : RawCollision2D<RawTriangleCollider2D, RawCircleCollider2D>
    {
        public override bool Overlap(RawTriangleCollider2D triangle, RawCircleCollider2D circle, Vector2 bodiesRay)
        {
            Vector3[] rays = new Vector3[3];
            float[] raysDotN = new float[3];
            float[] raysDotE = new float[3];
            int belowEdgeCount = 0;
            for (int i = 0; i < 3; i++)
            {
                rays[i] = circle.Body.Position - triangle.Verts[i];
                raysDotN[i] = Vector2.Dot(rays[i], triangle.Normals[i]);
                raysDotE[i] = Vector2.Dot(rays[i], triangle.Edges[i]) / triangle.Lengths[i];
                belowEdgeCount += raysDotN[i] <= 0f ? 1 : 0;
            }
            switch (belowEdgeCount)
            {
                case 3:
                    return true;
                case 2:
                    int aboveEdgeIndex = raysDotN[0] > 0f ? 0 : (raysDotN[1] > 0f ? 1 : 2);
                    if (raysDotE[aboveEdgeIndex] >= 0f && raysDotE[aboveEdgeIndex] <= triangle.Lengths[aboveEdgeIndex])
                    {
                        return raysDotN[aboveEdgeIndex] <= circle.Radius;
                    }
                    else
                    {
                        int vertIndex = raysDotE[aboveEdgeIndex] < 0f ? aboveEdgeIndex : (aboveEdgeIndex + 1) % 3;
                        return rays[vertIndex].sqrMagnitude <= circle.RadiusSquared;
                    }
                case 1:
                    {
                        int belowEdgeIndex = raysDotN[0] <= 0f ? 0 : (raysDotN[1] <= 0f ? 1 : 2);
                        int vertIndex = (belowEdgeIndex + 2) % 3;
                        return rays[vertIndex].sqrMagnitude <= circle.RadiusSquared;
                    }
                default:
                    return false;
            }
        }
    }

    public class RawBoxCircleCollision2D : RawCollision2D<RawBoxCollider2D, RawCircleCollider2D>
    {
        public override bool Overlap(RawBoxCollider2D box, RawCircleCollider2D circle, Vector2 bodiesRay)
        {
            Vector2 ray = box.RotMT.MultiplyVector(bodiesRay);
            float absRayX = Mathf.Abs(ray.x);
            float absRayY = Mathf.Abs(ray.y);
            bool onEdgeX = absRayX <= box.HalfSize.x;
            bool onEdgeY = absRayY <= box.HalfSize.y;
            if (onEdgeX && onEdgeY)
                return true;
            else if (onEdgeX)
                return absRayY <= box.HalfSize.y + circle.Radius;
            else if (onEdgeY)
                return absRayX <= box.HalfSize.x + circle.Radius;
            else
            {
                Vector2[] n = box.RotM.GetColumns();
                float[] signs = new float[] { Mathf.Sign(ray.x), Mathf.Sign(ray.y) };
                Vector2 cornerRay = n[0] * signs[0] * box.HalfSize.x + n[1] * signs[1] * box.HalfSize.y;
                Vector2 cornerPos = box.Body.Position + cornerRay;
                Vector2 cornerCircleRay = circle.Body.Position - cornerPos;
                return cornerCircleRay.x * cornerCircleRay.x + cornerCircleRay.y * cornerCircleRay.y <= circle.RadiusSquared;
            }
        }
    }

    public class RawCirclesCollision2D : RawCollision2D<RawCircleCollider2D, RawCircleCollider2D>
    {
        public static float basicDiameterSquared;

        public override bool Overlap(RawCircleCollider2D circle1, RawCircleCollider2D circle2, Vector2 bodiesRay)
        {
            float radiusSum = circle1.Radius + circle1.Radius;
            return bodiesRay.x * bodiesRay.x + bodiesRay.y * bodiesRay.y <= radiusSum * radiusSum;
        }

        public bool OverlapInt(RawCircleCollider2D circle1, RawCircleCollider2D circle2, Vector2Int bodiesRayInt)
        {
            int radiusSumInt = circle1.RadiusInt + circle1.RadiusInt;
            return bodiesRayInt.x * bodiesRayInt.x + bodiesRayInt.y * bodiesRayInt.y <= radiusSumInt * radiusSumInt;
        }

        public bool BasicRadiusOverlap(RawCircleCollider2D circle1, RawCircleCollider2D circle2, Vector2 bodiesRay)
        {
            return bodiesRay.x * bodiesRay.x + bodiesRay.y * bodiesRay.y <= basicDiameterSquared;
        }
    } 
}
