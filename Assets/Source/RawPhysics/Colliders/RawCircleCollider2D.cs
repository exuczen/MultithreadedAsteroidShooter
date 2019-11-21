namespace RawPhysics
{
    public class RawCircleCollider2D : RawCollider2D
    {
        private readonly float radius = default;
        private readonly float radiusSquared = default;

        public float Radius { get => radius; }
        public float RadiusSquared { get => radiusSquared; }

        public RawCircleCollider2D(float radius)
        {
            shape = RawColliderShape2D.Circle;
            this.radius = radius;
            radiusSquared = radius * radius;
        }

        protected override void OnUpdate() { }
    }
}
