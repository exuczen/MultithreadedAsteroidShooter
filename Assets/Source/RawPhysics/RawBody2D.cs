using System;
using UnityEngine;
using Mindpower;

namespace RawPhysics
{
    public abstract class RawBody2D
    {
        public int threadCellIndex;
        public int collCellIndex;
        public bool isInCameraView;

        private Vector2 position;
        //private Vector2Int positionInt;
        private Vector2 velocity;
        private Vector3 eulerAngles;
        private float angularVelocity;
        private float rotationAngleRad;
        protected Bounds2 bounds;
        private Vector2Int sizeInt;
        private Vector2Int halfSizeInt;
        private Color spriteColor;
        private float respawnTime;
        private float lifeTime;
        private float lifeStartTime;
        protected bool respawnable;
        protected bool explosive;

        protected Transform transform;
        private SpriteRenderer spriteRenderer;

        protected RawCollider2D collider;
        public RawCollider2D Collider { get => collider; }
        public RawColliderShape2D ColliderShape { get => collider.Shape; }

        public Bounds2 Bounds { get => bounds; }
        public Quaternion Rotation { get => Quaternion.Euler(eulerAngles); }
        public float RotationAngleRad { get => rotationAngleRad; }
        //public Vector2 Position { get => position; set => position = value; }
        public Vector2 Position
        {
            get => position;
            set {
                position = value;
                //positionInt.x = (int)(position.x * Const.FloatToIntFactor);
                //positionInt.y = (int)(position.y * Const.FloatToIntFactor);
            }
        }
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
            sizeInt.x = (int)(bounds.Size.x * Const.FloatToIntFactor);
            sizeInt.y = (int)(bounds.Size.y * Const.FloatToIntFactor);
            halfSizeInt.x = sizeInt.x >> 1;
            halfSizeInt.y = sizeInt.y >> 1;
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
            //return bounds.Intersects(body.bounds);
        }

        //public bool BoundsOverlapInt(RawBody2D body, out Vector2Int bodiesRayInt)
        //{
        //    bodiesRayInt = body.positionInt - positionInt;
        //    return
        //        Math.Abs(bodiesRayInt.x) < body.halfSizeInt.x + halfSizeInt.x &&
        //        Math.Abs(bodiesRayInt.y) < body.halfSizeInt.y + halfSizeInt.y;
        //}


        public void UpdateMotion(float time, float deltaTime)
        {
            position += velocity * deltaTime;
            rotationAngleRad += angularVelocity * deltaTime;
            bounds.Center = position;
            //positionInt.x = (int)(position.x * Const.FloatToIntFactor);
            //positionInt.y = (int)(position.y * Const.FloatToIntFactor);
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
