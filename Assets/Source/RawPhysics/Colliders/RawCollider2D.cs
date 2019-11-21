using UnityEngine;

namespace RawPhysics
{
    public enum RawColliderShape2D
    {
        Circle,
        Box,
        Triangle
    }

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
}
