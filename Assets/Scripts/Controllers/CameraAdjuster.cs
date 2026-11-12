using UnityEngine;

public class CameraAdjuster : MonoBehaviour
{
    public float targetAspect = 9f / 16f; // 默认宽高比
    public float baseOrthographicSize = 17.5f; // 默认相机 Size

    private Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();
        AdjustCamera();
    }

    private void AdjustCamera()
    {
        float windowAspect = (float)Screen.width / Screen.height;
        float scaleHeight = windowAspect / targetAspect;

        // 如果当前屏幕比设计比例更宽
        if (scaleHeight < 1.0f)
        {
            // 为了保证宽度能完整显示，把相机拉远（放大 Size）
            cam.orthographicSize = baseOrthographicSize / scaleHeight;
        }
        else
        {
            cam.orthographicSize = baseOrthographicSize;
        }
    }
}