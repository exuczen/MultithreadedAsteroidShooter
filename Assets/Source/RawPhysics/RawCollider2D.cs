using MustHave.Utilities;
using UnityEngine;

namespace RawPhysics
{
    public enum RawColliderShape2D
    {
        Circle,
        Box,
        Triangle
    }

    //[StructLayout(LayoutKind.Explicit)]
    //public struct DerivedRawCollider2D
    //{
    //    [FieldOffset(0)] public RawBoxCollider2D boxCollider;
    //    [FieldOffset(0)] public RawCircleCollider2D circleCollider;
    //    [FieldOffset(0)] public RawTriangleCollider2D triangleCollider;
    //}

    public abstract class RawCollider2D
    {
        protected RawColliderShape2D shape;
        public RawBody2D Body { get; set; }
        public RawColliderShape2D Shape { get => shape; }

        protected abstract void OnUpdate();

        public void Update()
        {
            OnUpdate();
        }
    }

    public class RawCircleCollider2D : RawCollider2D
    {
        private float radius;
        private float radiusSquared;
        private int radiusInt;

        public float Radius { get => radius; }
        public int RadiusInt { get => radiusInt; }
        public float RadiusSquared { get => radiusSquared; }

        public RawCircleCollider2D(float radius)
        {
            shape = RawColliderShape2D.Circle;
            this.radius = radius;
            radiusSquared = radius * radius;
            radiusInt = (int)(radius * Const.FloatToIntFactor);
        }

        protected override void OnUpdate() { }
    }

    public class RawPolygonCollider2D : RawCollider2D
    {
        protected Matrix2 rotM;
        protected Matrix2 rotMT;

        public Matrix2 RotM { get => rotM; }
        public Matrix2 RotMT { get => rotMT; }

        protected override void OnUpdate()
        {
            rotM.SetRotation(Body.RotationAngleRad);
            rotMT = rotM.Transpose();
        }
    }

    public class RawBoxCollider2D : RawPolygonCollider2D
    {
        private Vector2 size;
        private Vector2 halfSize;

        public Vector2 Size { get => size; }
        public Vector2 HalfSize { get => halfSize; }

        public RawBoxCollider2D(Vector2 size)
        {
            shape = RawColliderShape2D.Box;
            this.size = size;
            halfSize = size / 2f;
        }

        protected override void OnUpdate()
        {
            base.OnUpdate();
        }
    }

    public class RawTriangleCollider2D : RawPolygonCollider2D
    {
        private Vector2[] localVerts = new Vector2[3];
        private Vector2[] verts = new Vector2[3];
        private Vector2[] edges = new Vector2[3];
        private Vector2[] normals = new Vector2[3];
        private float[] lengths = new float[3];

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