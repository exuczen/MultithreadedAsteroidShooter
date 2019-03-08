using System;
using UnityEngine;

namespace DC
{
    public struct Matrix2 : IEquatable<Matrix2>
    {
        private float m00;
        private float m01;
        private float m10;
        private float m11;

        public Matrix2(float m00, float m01, float m10, float m11)
        {
            this.m00 = m00;
            this.m01 = m01;
            this.m10 = m10;
            this.m11 = m11;
        }

        private float[] Values { get => new float[4] { m00, m01, m10, m11 }; }

        public static Matrix2 Rotation(float angleRad)
        {
            Matrix2 m = new Matrix2();
            m.SetRotation(angleRad);
            return m;
        }

        public void SetRotation(float angleRad)
        {
            float cosAngle = Mathf.Cos(angleRad);
            float sinAngle = Mathf.Sin(angleRad);
            m00 = cosAngle;
            m01 = -sinAngle;
            m10 = sinAngle;
            m11 = cosAngle;
        }

        public Matrix2 Transpose()
        {
            return new Matrix2(m00, m10, m01, m11);
        }

        public Vector2 MultiplyVector(Vector2 vector)
        {
            return new Vector2(
                m00 * vector.x + m01 * vector.y,
                m10 * vector.x + m11 * vector.y
            );
        }

        public Vector2[] GetRows()
        {
            return new Vector2[] {
                new Vector2(m00, m01), new Vector2(m10, m11)
            };
        }

        public Vector2[] GetColumns()
        {
            return new Vector2[] {
                new Vector2(m00, m10), new Vector2(m01, m11)
            };
        }

        public Vector2 GetRow(int index)
        {
            float[] v = Values;
            int begIndex = index << 1;
            return new Vector2(v[begIndex], v[begIndex + 1]);
        }

        public Vector2 GetColumn(int index)
        {
            float[] v = Values;
            return new Vector2(v[index], v[2 + index]);
        }

        public bool Equals(Matrix2 other)
        {
            Matrix4x4 m;
            return
                m00 == other.m00 &&
                m01 == other.m01 &&
                m10 == other.m10 &&
                m11 == other.m11;
        }
    }
}
