using UnityEngine;

[RequireComponent(typeof(Camera))]
public class FixedAspectCamera : MonoBehaviour
{
    private Camera cam;
    private int lastWidth;
    private int lastHeight;

    // 1080x1920 = 9:16
    public float targetAspect = 9f / 16f;

    void Awake()
    {
        cam = GetComponent<Camera>();
        Apply();
    }

    void Update()
    {
        if (Screen.width != lastWidth || Screen.height != lastHeight)
        {
            Apply();
        }
    }

    void Apply()
    {
        lastWidth = Screen.width;
        lastHeight = Screen.height;

        float windowAspect = (float)Screen.width / Screen.height;
        float scaleHeight = windowAspect / targetAspect;

        Rect rect = new Rect(0, 0, 1, 1);

        if (scaleHeight < 1.0f)
        {
            // 창이 더 좁음 -> 위아래는 꽉 차고, 좌우에 검은 여백
            rect.width = 1.0f;
            rect.height = scaleHeight;
            rect.x = 0;
            rect.y = (1.0f - scaleHeight) / 2.0f;
        }
        else
        {
            // 창이 더 넓음 -> 좌우는 꽉 차고, 위아래에 검은 여백
            float scaleWidth = 1.0f / scaleHeight;
            rect.width = scaleWidth;
            rect.height = 1.0f;
            rect.x = (1.0f - scaleWidth) / 2.0f;
            rect.y = 0;
        }

        cam.rect = rect;
    }
}