using UnityEngine;

/// <summary>
/// 车辆高度锁定 - 防止车辆在Y轴上移动
/// </summary>
public class VehicleHeightLock : MonoBehaviour
{
    [Header("高度设置")]
    [Tooltip("锁定的高度（Y轴位置）")]
    public float lockedHeight = 0f;

    [Tooltip("是否在开始时自动设置当前高度")]
    public bool useCurrentHeight = true;

    [Tooltip("是否启用高度锁定")]
    public bool enableHeightLock = true;

    [Header("地形适应")]
    [Tooltip("是否根据地形高度自动调整")]
    public bool adaptToTerrain = false;

    [Tooltip("地面检测层")]
    public LayerMask groundLayer;

    [Tooltip("距离地面的高度")]
    public float heightAboveGround = 0.5f;

    [Tooltip("地面检测距离")]
    public float raycastDistance = 10f;

    private Rigidbody rb;
    private Transform cachedTransform;

    void Start()
    {
        cachedTransform = transform;
        rb = GetComponent<Rigidbody>();

        // 如果使用当前高度，记录初始Y位置
        if (useCurrentHeight)
        {
            lockedHeight = cachedTransform.position.y;
        }

        // 如果有 Rigidbody，锁定Y轴旋转和位置
        if (rb != null)
        {
            // 保持其他约束，只添加Y轴位置锁定
            rb.constraints = RigidbodyConstraints.FreezePositionY |
                            RigidbodyConstraints.FreezeRotationX |
                            RigidbodyConstraints.FreezeRotationZ;
        }
    }

    void LateUpdate()
    {
        if (!enableHeightLock) return;

        Vector3 currentPos = cachedTransform.position;

        if (adaptToTerrain)
        {
            // 地形适应模式：根据地面高度调整
            float targetHeight = GetTerrainHeight();
            currentPos.y = targetHeight;
        }
        else
        {
            // 固定高度模式：锁定到指定高度
            currentPos.y = lockedHeight;
        }

        cachedTransform.position = currentPos;
    }

    /// <summary>
    /// 获取地形高度
    /// </summary>
    float GetTerrainHeight()
    {
        RaycastHit hit;
        Vector3 rayOrigin = cachedTransform.position + Vector3.up * 5f;

        // 向下发射射线检测地面
        if (Physics.Raycast(rayOrigin, Vector3.down, out hit, raycastDistance, groundLayer))
        {
            return hit.point.y + heightAboveGround;
        }

        // 如果没有检测到地面，使用锁定高度
        return lockedHeight;
    }

    /// <summary>
    /// 设置锁定高度
    /// </summary>
    public void SetLockedHeight(float height)
    {
        lockedHeight = height;
    }

    /// <summary>
    /// 启用/禁用高度锁定
    /// </summary>
    public void SetHeightLock(bool enabled)
    {
        enableHeightLock = enabled;
    }

    /// <summary>
    /// 重置到当前高度
    /// </summary>
    public void ResetToCurrentHeight()
    {
        lockedHeight = cachedTransform.position.y;
    }

    void OnDrawGizmosSelected()
    {
        if (!enableHeightLock) return;

        Transform t = Application.isPlaying ? cachedTransform : transform;

        // 绘制锁定高度平面
        Gizmos.color = new Color(0f, 1f, 0f, 0.3f);
        Vector3 planeCenter = t.position;
        planeCenter.y = lockedHeight;

        // 绘制水平网格
        float gridSize = 10f;
        for (int i = -5; i <= 5; i++)
        {
            Vector3 start = planeCenter + new Vector3(i * 2f, 0, -gridSize);
            Vector3 end = planeCenter + new Vector3(i * 2f, 0, gridSize);
            Gizmos.DrawLine(start, end);

            start = planeCenter + new Vector3(-gridSize, 0, i * 2f);
            end = planeCenter + new Vector3(gridSize, 0, i * 2f);
            Gizmos.DrawLine(start, end);
        }

        // 绘制当前位置到锁定高度的连线
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(t.position, planeCenter);

#if UNITY_EDITOR
        UnityEditor.Handles.Label(
            planeCenter + Vector3.up * 1f,
            $"锁定高度: {lockedHeight:F2}m\n当前高度: {t.position.y:F2}m"
        );
#endif
    }
}