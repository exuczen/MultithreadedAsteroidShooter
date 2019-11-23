using UnityEngine;

namespace RawPhysics
{
    public class RawBoxCircleCollision2D : RawCollision2D<RawBoxCollider2D, RawCircleCollider2D>
    {
        public override bool Overlap(RawBoxCollider2D box, RawCircleCollider2D circle, Vector2 bodiesRay)
        {
            Vector2 ray = box.RotMT.MultiplyVector(bodiesRay);
            float absRayX = Mathf.Abs(ray.x);
            float absRayY = Mathf.Abs(ray.y);
            bool onEdgeX = absRayX <= box.HalfSize.x;
            bool onEdgeY = absRayY <= box.HalfSize.y;
            if (onEdgeX && onEdgeY)
                return true;
            else if (onEdgeX)
                return absRayY <= box.HalfSize.y + circle.Radius;
            else if (onEdgeY)
                return absRayX <= box.HalfSize.x + circle.Radius;
            else
            {
                Vector2[] n = box.RotM.GetColumns();
                float[] signs = new float[] { Mathf.Sign(ray.x), Mathf.Sign(ray.y) };
                Vector2 cornerRay = n[0] * signs[0] * box.HalfSize.x + n[1] * signs[1] * box.HalfSize.y;
                Vector2 cornerPos = box.Body.Position + cornerRay;
                Vector2 cornerCircleRay = circle.Body.Position - cornerPos;
                return cornerCircleRay.x * cornerCircleRay.x + cornerCircleRay.y * cornerCircleRay.y <= circle.RadiusSquared;
            }
        }
    }
}
