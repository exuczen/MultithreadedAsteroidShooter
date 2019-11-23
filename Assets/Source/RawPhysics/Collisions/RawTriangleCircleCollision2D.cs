using UnityEngine;

namespace RawPhysics
{
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
}
