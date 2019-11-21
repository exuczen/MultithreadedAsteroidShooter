using UnityEngine;

namespace RawPhysics
{
    public class RawTriangleCollider2D : RawPolygonCollider2D
    {
        private readonly Vector2[] localVerts = new Vector2[3];
        private readonly Vector2[] verts = new Vector2[3];
        private readonly Vector2[] edges = new Vector2[3];
        private readonly Vector2[] normals = new Vector2[3];
        private readonly float[] lengths = new float[3];

        public Vector2[] Verts { get => verts; }
        public Vector2[] Edges { get => edges; }
        public Vector2[] Normals { get => normals; }
        public float[] Lengths { get => lengths; }

        public RawTriangleCollider2D(Vector2 v0, Vector2 v1, Vector2 v2)
        {
            shape = RawColliderShape2D.Triangle;
            localVerts[0] = v0;
            localVerts[1] = v1;
            localVerts[2] = v2;
            for (int i = 0; i < 3; i++)
            {
                lengths[i] = (localVerts[(i + 1) % 3] - localVerts[i]).magnitude;
            }
        }

        protected override void OnUpdate()
        {
            base.OnUpdate();
            for (int i = 0; i < 3; i++)
            {
                verts[i] = Body.Position + rotM.MultiplyVector(localVerts[i]);
            }
            for (int i = 0; i < 3; i++)
            {
                edges[i] = verts[(i + 1) % 3] - verts[i];
                normals[i].Set(edges[i].y, -edges[i].x);
                normals[i] = normals[i] / lengths[i];
            }
        }
    }
}
