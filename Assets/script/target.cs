using UnityEngine;

public class MinimapViewportMarker : MonoBehaviour
{
    [Header("UI")]
    public RectTransform marker;      // 橘色点
    public RectTransform mapRoot;     // circlemask（与RawImage同尺寸的区域）

    [Header("World")]
    public Camera minimapCam;         // 小地图相机（输出到RenderTexture的那台）
    public Transform target;          // 红色物体（要对齐的那个）

    [Header("Clamp")]
    public bool clampToEdge = true;   // 超出小地图范围时贴边
    public float edgePadding = 2f;    // 贴边留点边距（像素）

    void Reset()
    {
        marker = GetComponent<RectTransform>();
    }

    void LateUpdate()
    {
        if (!marker || !mapRoot || !minimapCam || !target) return;

        // 1) 世界坐标 -> 小地图相机视口坐标(0~1)
        Vector3 v = minimapCam.WorldToViewportPoint(target.position);

        // v.z < 0 说明在相机后面（一般不会发生在俯视小地图，但加个保险）
        if (v.z < 0f)
        {
            marker.gameObject.SetActive(false);
            return;
        }
        marker.gameObject.SetActive(true);

        // 2) viewport(0~1) -> mapRoot anchoredPosition
        float w = mapRoot.rect.width;
        float h = mapRoot.rect.height;

        float x = (v.x - 0.5f) * w;
        float y = (v.y - 0.5f) * h;

        if (clampToEdge)
        {
            float halfW = w * 0.5f - edgePadding;
            float halfH = h * 0.5f - edgePadding;
            x = Mathf.Clamp(x, -halfW, halfW);
            y = Mathf.Clamp(y, -halfH, halfH);
        }

        marker.anchoredPosition = new Vector2(x, y);
    }
}
