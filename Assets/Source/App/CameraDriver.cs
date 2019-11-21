using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraDriver : MonoBehaviour
{
    private new Camera camera = default;
    private float cameraInitialOrthoSize = default;

    private const float SMOOTH_FACTOR = 0.08f;
    private float distance = default;

    private void Start()
    {
        camera = GetComponent<Camera>();
        cameraInitialOrthoSize = camera.orthographicSize;
        distance = transform.localPosition.z;
        ResetCameraPosition();
    }

    public void FollowTarget(Transform target)
    {
        Vector3 targetPos = target.position;
        Vector3 cameraPos = transform.position;
        cameraPos.z = targetPos.z;
        float dist = Vector2.Distance(cameraPos, targetPos);
        float rMax = 2f * Mathf.Max(1f, 0.5f + 0.5f * camera.orthographicSize / cameraInitialOrthoSize);
        float t = Mathf.Max(SMOOTH_FACTOR, 1f - rMax / dist);
        cameraPos = Vector2.Lerp(cameraPos, targetPos, t);
        cameraPos.z = distance;
        transform.position = cameraPos;
    }

    public void ResetCameraPosition()
    {
        camera.transform.position = new Vector3(0f, 0f, distance);
    }

}
