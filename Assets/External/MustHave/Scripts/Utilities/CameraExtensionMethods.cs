using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MustHave.Utilities
{
    public static class CameraExtensionMethods
    {
        public static Bounds2 GetOrthographicBounds2WithOffset(this Camera camera, float viewportOffset)
        {
            float offset = viewportOffset * camera.orthographicSize;
            return new Bounds2(camera.transform.position,
                new Vector2(camera.orthographicSize * Screen.width / Screen.height + offset, camera.orthographicSize + offset) * 2f);
        }

        public static Bounds2 GetOrthographicBounds2(this Camera camera)
        {
            return new Bounds2(camera.transform.position,
                new Vector2(camera.orthographicSize * Screen.width / Screen.height, camera.orthographicSize) * 2f);
        }

        public static float GetTanHalfFovFromProjectionMatrix(this Camera camera)
        {
            return camera.projectionMatrix.GetTanHalfFovFromProjection();
        }

        public static float GetFovFromProjectionMatrix(this Camera camera)
        {
            return camera.projectionMatrix.GetFovFromProjection();
        }
    }
}
