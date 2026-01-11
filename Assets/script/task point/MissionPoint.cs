using UnityEngine;

/// <summary>
/// 任务点 - 代表地图上的一个可交互任务点
/// </summary>
public class MissionPoint : MonoBehaviour
{
    [Header("任务点设置")]
    [Tooltip("任务点ID（唯一标识）")]
    public int pointID;

    [Tooltip("任务点名称")]
    public string pointName = "任务点";

    [Header("检测设置")]
    [Tooltip("玩家进入范围的距离（用于打卡）")]
    public float activationDistance = 3f;

    [Tooltip("开始游戏的触发距离（等待状态时有效）")]
    public float startGameDistance = 5f;

    // 组件引用
    private Renderer pointRenderer;

    // 状态
    private bool isActive = false;
    private bool isCompleted = false;

    void Start()
    {
        // 获取渲染器
        pointRenderer = GetComponent<Renderer>();

        if (pointRenderer == null)
        {
            Debug.LogWarning($"[{name}] 缺少 Renderer 组件！");
        }

        // 初始状态：全部可见（等待开始游戏）
        SetVisibility(true);
    }

    /// <summary>
    /// 激活任务点（游戏开始后被选中的点）
    /// </summary>
    public void Activate()
    {
        isActive = true;
        isCompleted = false;
        SetVisibility(true);
    }

    /// <summary>
    /// 停用任务点（游戏开始后未被选中的点）
    /// </summary>
    public void Deactivate()
    {
        isActive = false;
        SetVisibility(false);
    }

    /// <summary>
    /// 显示任务点（等待开始时显示所有点）
    /// </summary>
    public void Show()
    {
        SetVisibility(true);
    }

    /// <summary>
    /// 隐藏任务点
    /// </summary>
    public void Hide()
    {
        SetVisibility(false);
    }

    /// <summary>
    /// 完成任务点
    /// </summary>
    public void Complete()
    {
        isCompleted = true;

        // 完成后隐藏任务点
        SetVisibility(false);

        // 通知任务管理器
        MissionManager manager = FindObjectOfType<MissionManager>();
        if (manager != null)
        {
            manager.OnPointCompleted(this);
        }
    }

    /// <summary>
    /// 检查玩家是否在范围内（用于任务点打卡）
    /// </summary>
    public bool CheckPlayerInRange(Transform playerTransform)
    {
        if (!isActive || isCompleted) return false;

        float distance = Vector3.Distance(transform.position, playerTransform.position);
        return distance <= activationDistance;
    }

    /// <summary>
    /// 检查玩家是否在开始范围内（等待状态时所有点都可以触发）
    /// </summary>
    public bool CheckPlayerInStartRange(Transform playerTransform)
    {
        float distance = Vector3.Distance(transform.position, playerTransform.position);
        return distance <= startGameDistance;
    }

    /// <summary>
    /// 设置可见性
    /// </summary>
    void SetVisibility(bool visible)
    {
        if (pointRenderer != null)
        {
            pointRenderer.enabled = visible;
        }
    }

    /// <summary>
    /// 获取激活状态
    /// </summary>
    public bool IsActive()
    {
        return isActive;
    }

    /// <summary>
    /// 获取完成状态
    /// </summary>
    public bool IsCompleted()
    {
        return isCompleted;
    }

    // Gizmos 显示范围
    void OnDrawGizmosSelected()
    {
        // 显示两个范围圈
        // 外圈：开始游戏范围（黄色）
        Gizmos.color = new Color(1f, 1f, 0f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, startGameDistance);

        // 内圈：打卡范围（绿色）
        Gizmos.color = new Color(0f, 1f, 0f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, activationDistance);

#if UNITY_EDITOR
        UnityEditor.Handles.Label(
            transform.position + Vector3.up * 2f,
            $"任务点 #{pointID}\n{pointName}\n开始范围: {startGameDistance}m\n打卡范围: {activationDistance}m"
        );
#endif
    }
}