using MustHave.Utilities;

namespace RawPhysics
{
    public class RawPolygonCollider2D : RawCollider2D
    {
        protected Matrix2 rotM = default;
        protected Matrix2 rotMT = default;

        public Matrix2 RotM { get => rotM; }
        public Matrix2 RotMT { get => rotMT; }

        protected override void OnUpdate()
        {
            rotM.SetRotation(Body.RotationAngleRad);
            rotMT = rotM.Transpose();
        }
    }
}
