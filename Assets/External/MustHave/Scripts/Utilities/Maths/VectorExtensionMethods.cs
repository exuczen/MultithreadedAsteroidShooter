using UnityEngine;

namespace MustHave.Utilities
{
    public static class VectorExtensionMethods
    {
        public static Vector3 Modulo(this Vector3 v, float modulo)
        {
            return new Vector3(
                v.x % modulo,
                v.y % modulo,
                v.z % modulo
                );
        }

        public static bool IsInCameraView(this Vector3 pos, Camera camera, float viewportOffsetX = 0f, float viewportOffsetY = 0f)
        {
            Vector3 viewportPos = camera.WorldToViewportPoint(pos);
            return
                viewportPos.x > -viewportOffsetX && viewportPos.x < 1f + viewportOffsetX &&
                viewportPos.y > -viewportOffsetY && viewportPos.y < 1f + viewportOffsetY;
        }

        public static bool IsInCameraView(this Vector2 pos, Camera camera, float viewportOffsetX = 0f, float viewportOffsetY = 0f)
        {
            Vector2 viewportPos = camera.WorldToViewportPoint(pos);
            return
                viewportPos.x > -viewportOffsetX && viewportPos.x < 1f + viewportOffsetX &&
                viewportPos.y > -viewportOffsetY && viewportPos.y < 1f + viewportOffsetY;
        }
    }
}
