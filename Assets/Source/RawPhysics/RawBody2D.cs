using System;
using UnityEngine;
using MustHave;

namespace RawPhysics
{
    public abstract class RawBody2D
    {
        public int threadCellIndex = default;
        public int collCellIndex = default;
        public bool isInCameraView = default;

        private Vector2 position = default;
        private Vector2 velocity = default;
        private Vector3 eulerAngles = default;
        private float angularVelocity = default;
        private float rotationAngleRad = default;
        protected Bounds2 bounds = default;
        private Color spriteColor = default;
        private float respawnTime = default;
        private float lifeTime = default;
        private float lifeStartTime = default;
        protected bool respawnable = default;
        protected bool explosive = default;

        protected Transform transform = default;
        private SpriteRenderer spriteRenderer = default;

        protected RawCollider2D collider;
        public RawCollider2D Collider { get => collider; }
        public RawColliderShape2D ColliderShape { get => collider.Shape; }
        public Bounds2 Bounds { get => bounds; }
        public Quaternion Rotation { get => Quaternion.Euler(eulerAngles); }
        public float RotationAngleRad { get => rotationAngleRad; }
        public Vector2 Position { get => position; set { position = value; } }
        public Vector2 Velocity { get => velocity; }
        public float AngularVelocity { get => angularVelocity; }
        public float RespawnTime { get => respawnTime; set => respawnTime = value; }
        public bool Explosive { get => explosive; }
        public bool Respawnable { get => respawnable; }
        public float LifeTime { get => lifeTime; set => lifeTime = value; }
        public float LifeStartTime { get => lifeStartTime; }

        public RawBody2D(Vector2 position, Vector3 eulerAngles, Bounds2 bounds)
        {
            this.position = position;
            this.eulerAngles = eulerAngles;
            this.bounds = bounds;
            rotationAngleRad = eulerAngles.z * Mathf.Deg2Rad;
            threadCellIndex = -1;
            collCellIndex = -1;
            lifeTime = -1;
            respawnable = false;
            explosive = false;
            isInCameraView = false;
            spriteColor = Color.black;
            bounds.Center = position;
        }

        public void Reset()
        {
            position = Vector2.zero;
            eulerAngles = Vector3.zero;
            rotationAngleRad = 0f;
            SetVelocities(Vector2.zero, 0f);
            SetSpriteColor(Color.white);
            SetGameObjectData();
        }

        public void SetLifeTime(float lifeStartTime, float lifeTime)
        {
            this.lifeStartTime = lifeStartTime;
            this.lifeTime = lifeTime;
        }

        public void SetCollider(RawCollider2D collider)
        {
            this.collider = collider;
            collider.Body = this;
        }

        public void SetVelocities(Vector2 velocity, float angularVelocity)
        {
            this.velocity = velocity;
            this.angularVelocity = angularVelocity;
        }

        public bool BoundsOverlap(RawBody2D body, out Vector2 bodiesRay)
        {
            bodiesRay = body.position - position;
            return
                Mathf.Abs(bodiesRay.x) < body.bounds.Extents.x + bounds.Extents.x &&
                Mathf.Abs(bodiesRay.y) < body.bounds.Extents.y + bounds.Extents.y;
        }

        public void UpdateMotion(float deltaTime)
        {
            position += velocity * deltaTime;
            rotationAngleRad += angularVelocity * deltaTime;
            bounds.Center = position;
            collider.Update();
        }

        public void SetGameObjectData()
        {
            if (!transform)
                SetGameObject();

            eulerAngles.z = rotationAngleRad * Mathf.Rad2Deg;
            transform.position = position;
            transform.eulerAngles = eulerAngles;
            spriteRenderer.color = spriteColor;
        }

        public void SetSpriteColor(Color c)
        {
            spriteColor = c;
        }

        public void RemoveGameObject()
        {
            if (transform)
            {
                RemoveGameObjectInstance();
            }
            transform = null;
            spriteRenderer = null;
            isInCameraView = false;
        }

        public void RemoveGameObjectAfterCollision(RawBody2D other)
        {
            RemoveGameObject();
            OnDestroyWithCollision(other);
        }

        public void SetGameObject()
        {
            transform = GetGameObjectInstance(out spriteRenderer).transform;
        }

        protected abstract void RemoveGameObjectInstance();

        protected abstract GameObject GetGameObjectInstance(out SpriteRenderer spriteRenderer);

        protected virtual void OnDestroyWithCollision(RawBody2D other) { }

        public virtual void Explode(Vector2 explosionPos) { }

        public virtual void Respawn() { }
    }
}
