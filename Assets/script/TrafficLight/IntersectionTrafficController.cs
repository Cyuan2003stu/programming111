using UnityEngine;

/// <summary>
/// 路口红绿灯控制器 - 管理一个路口的多个红绿灯协调工作
/// </summary>
public class IntersectionTrafficController : MonoBehaviour
{
    [System.Serializable]
    public class TrafficLightGroup
    {
        [Tooltip("这组红绿灯的名称（如：南北向、东西向）")]
        public string groupName = "南北向";

        [Tooltip("这组包含的所有红绿灯")]
        public TrafficLight[] lights;

        [Tooltip("绿灯持续时间")]
        public float greenDuration = 15f;

        [Tooltip("黄灯持续时间")]
        public float yellowDuration = 3f;

        [Tooltip("红灯持续时间（通常由其他组的绿灯+黄灯时间决定）")]
        public float redDuration = 18f;
    }

    [Header("路口红绿灯分组")]
    [Tooltip("将同一方向的红绿灯分为一组")]
    public TrafficLightGroup[] lightGroups;

    [Header("初始设置")]
    [Tooltip("游戏开始时哪一组先绿灯（0=第一组，1=第二组...）")]
    public int initialGreenGroupIndex = 0;

    [Tooltip("开始前的延迟时间（秒）")]
    public float startDelay = 0f;

    [Header("全红灯阶段")]
    [Tooltip("是否启用全红灯阶段（所有方向都红灯的过渡期）")]
    public bool enableAllRedPhase = true;

    [Tooltip("全红灯阶段持续时间")]
    public float allRedDuration = 2f;

    [Header("调试")]
    public bool showDebugInfo = true;

    private int currentGreenGroupIndex = 0;
    private float phaseTimer = 0f;
    private TrafficLightPhase currentPhase = TrafficLightPhase.Green;

    private enum TrafficLightPhase
    {
        Green,    // 当前组绿灯
        Yellow,   // 当前组黄灯
        AllRed,   // 全红灯过渡
        Red       // 当前组红灯
    }

    void Start()
    {
        if (lightGroups == null || lightGroups.Length == 0)
        {
            Debug.LogError($"[{gameObject.name}] 没有配置红绿灯组！");
            enabled = false;
            return;
        }

        // 验证配置
        for (int i = 0; i < lightGroups.Length; i++)
        {
            if (lightGroups[i].lights == null || lightGroups[i].lights.Length == 0)
            {
                Debug.LogWarning($"[{gameObject.name}] 红绿灯组 {i} ({lightGroups[i].groupName}) 没有配置红绿灯！");
            }
        }

        currentGreenGroupIndex = Mathf.Clamp(initialGreenGroupIndex, 0, lightGroups.Length - 1);

        // 延迟启动
        if (startDelay > 0)
        {
            Invoke(nameof(Initialize), startDelay);
        }
        else
        {
            Initialize();
        }
    }

    void Initialize()
    {
        currentPhase = TrafficLightPhase.Green;
        phaseTimer = lightGroups[currentGreenGroupIndex].greenDuration;

        UpdateAllLights();

        if (showDebugInfo)
        {
            Debug.Log($"[{gameObject.name}] 路口红绿灯初始化完成，{lightGroups[currentGreenGroupIndex].groupName} 绿灯");
        }
    }

    void Update()
    {
        if (lightGroups == null || lightGroups.Length == 0) return;

        phaseTimer -= Time.deltaTime;

        if (phaseTimer <= 0)
        {
            SwitchToNextPhase();
        }
    }

    void SwitchToNextPhase()
    {
        switch (currentPhase)
        {
            case TrafficLightPhase.Green:
                // 绿灯 → 黄灯
                currentPhase = TrafficLightPhase.Yellow;
                phaseTimer = lightGroups[currentGreenGroupIndex].yellowDuration;

                if (showDebugInfo)
                {
                    Debug.Log($"[{gameObject.name}] {lightGroups[currentGreenGroupIndex].groupName} 切换到黄灯");
                }
                break;

            case TrafficLightPhase.Yellow:
                // 黄灯 → 全红灯（如果启用）或直接切换到下一组
                if (enableAllRedPhase)
                {
                    currentPhase = TrafficLightPhase.AllRed;
                    phaseTimer = allRedDuration;

                    if (showDebugInfo)
                    {
                        Debug.Log($"[{gameObject.name}] 进入全红灯阶段");
                    }
                }
                else
                {
                    SwitchToNextGroup();
                }
                break;

            case TrafficLightPhase.AllRed:
                // 全红灯 → 切换到下一组绿灯
                SwitchToNextGroup();
                break;
        }

        UpdateAllLights();
    }

    void SwitchToNextGroup()
    {
        // 切换到下一组
        currentGreenGroupIndex = (currentGreenGroupIndex + 1) % lightGroups.Length;
        currentPhase = TrafficLightPhase.Green;
        phaseTimer = lightGroups[currentGreenGroupIndex].greenDuration;

        if (showDebugInfo)
        {
            Debug.Log($"[{gameObject.name}] 切换到 {lightGroups[currentGreenGroupIndex].groupName} 绿灯");
        }
    }

    void UpdateAllLights()
    {
        for (int i = 0; i < lightGroups.Length; i++)
        {
            TrafficLightGroup group = lightGroups[i];

            if (group.lights == null) continue;

            TrafficLight.LightState targetState;

            if (i == currentGreenGroupIndex)
            {
                // 当前活跃组
                switch (currentPhase)
                {
                    case TrafficLightPhase.Green:
                        targetState = TrafficLight.LightState.Green;
                        break;
                    case TrafficLightPhase.Yellow:
                        targetState = TrafficLight.LightState.Yellow;
                        break;
                    case TrafficLightPhase.AllRed:
                        targetState = TrafficLight.LightState.Red;
                        break;
                    default:
                        targetState = TrafficLight.LightState.Red;
                        break;
                }
            }
            else
            {
                // 其他组保持红灯
                targetState = TrafficLight.LightState.Red;
            }

            // 更新这组的所有红绿灯
            foreach (var light in group.lights)
            {
                if (light != null)
                {
                    light.SetState(targetState);
                }
            }
        }
    }

    /// <summary>
    /// 手动切换到指定组的绿灯（用于调试或特殊情况）
    /// </summary>
    public void ForceGreenLight(int groupIndex)
    {
        if (groupIndex < 0 || groupIndex >= lightGroups.Length)
        {
            Debug.LogWarning($"[{gameObject.name}] 无效的组索引: {groupIndex}");
            return;
        }

        currentGreenGroupIndex = groupIndex;
        currentPhase = TrafficLightPhase.Green;
        phaseTimer = lightGroups[currentGreenGroupIndex].greenDuration;
        UpdateAllLights();

        if (showDebugInfo)
        {
            Debug.Log($"[{gameObject.name}] 强制切换到 {lightGroups[groupIndex].groupName} 绿灯");
        }
    }

    /// <summary>
    /// 暂停/恢复红绿灯切换
    /// </summary>
    public void SetPaused(bool paused)
    {
        enabled = !paused;
    }

    private void OnDrawGizmos()
    {
        if (lightGroups == null) return;

        // 在Scene视图中绘制路口控制器的信息
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(transform.position, Vector3.one * 2f);

#if UNITY_EDITOR
        // 显示当前状态
        string stateInfo = Application.isPlaying ?
            $"路口控制器\n当前: {lightGroups[currentGreenGroupIndex].groupName}\n阶段: {currentPhase}\n剩余: {phaseTimer:F1}s" :
            "路口控制器\n(运行时显示状态)";

        UnityEditor.Handles.Label(transform.position + Vector3.up * 3f, stateInfo);

        // 绘制到每组红绿灯的连线
        for (int i = 0; i < lightGroups.Length; i++)
        {
            if (lightGroups[i].lights != null)
            {
                Gizmos.color = Application.isPlaying && i == currentGreenGroupIndex ?
                    Color.green : Color.gray;

                foreach (var light in lightGroups[i].lights)
                {
                    if (light != null)
                    {
                        Gizmos.DrawLine(transform.position, light.transform.position);
                    }
                }
            }
        }
#endif
    }

    private void OnDrawGizmosSelected()
    {
        if (lightGroups == null) return;

        // 绘制详细信息
        for (int i = 0; i < lightGroups.Length; i++)
        {
            if (lightGroups[i].lights == null) continue;

            Color groupColor = Color.HSVToRGB(i / (float)lightGroups.Length, 0.8f, 1f);

            foreach (var light in lightGroups[i].lights)
            {
                if (light != null)
                {
                    Gizmos.color = groupColor;
                    Gizmos.DrawWireSphere(light.transform.position, 1.5f);

#if UNITY_EDITOR
                    UnityEditor.Handles.Label(
                        light.transform.position + Vector3.up * 2f,
                        $"{lightGroups[i].groupName}\n组{i}"
                    );
#endif
                }
            }
        }
    }
}