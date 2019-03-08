using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraDriver : MonoBehaviour
{
    private new Camera camera;

    private const float smoothFactor = 4f;

    private float cameraInitialOrthoSize;

    private void Start()
    {
        camera = GetComponent<Camera>();
        cameraInitialOrthoSize = camera.orthographicSize;
        ResetCameraPosition();
    }

    public void FollowTarget(Transform target)
    {
        Vector3 targetPos = target.position;
        Vector3 cameraPos = transform.position;
        cameraPos.z = targetPos.z;
        float dist = Vector2.Distance(cameraPos, targetPos);
        float rMax = 2f * Mathf.Max(1f, 0.5f + 0.5f * camera.orthographicSize / cameraInitialOrthoSize);
        float tMin = smoothFactor * Time.fixedDeltaTime;
        float t = Mathf.Max(tMin, 1f - rMax / dist);
        cameraPos = Vector2.Lerp(cameraPos, targetPos, t);
        cameraPos.z = -10f;
        transform.position = cameraPos;
    }

    public void ResetCameraPosition()
    {
        camera.transform.position = new Vector3(0f, 0f, -10f);
    }

}
