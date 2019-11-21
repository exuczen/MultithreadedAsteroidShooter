using UnityEngine;

namespace RawPhysics
{
    public class RawBoxCollider2D : RawPolygonCollider2D
    {
        private Vector2 size = default;
        private Vector2 halfSize = default;

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
}
