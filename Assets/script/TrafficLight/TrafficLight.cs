using UnityEngine;

public class TrafficLight : MonoBehaviour
{
    public enum LightState
    {
        Red,
        Yellow,
        Green
    }

    [Header("灯光设置")]
    public LightState currentState = LightState.Red;
    public Material redMaterial;
    public Material yellowMaterial;
    public Material greenMaterial;
    public Renderer lightRenderer;

    [Header("时间设置")]
    public float greenDuration = 10f;
    public float yellowDuration = 3f;
    public float redDuration = 10f;

    [Header("控制模式")]
    [Tooltip("是否由路口控制器统一管理（如果是，忽略自动切换）")]
    public bool controlledByIntersection = false;

    [Header("可选：实际灯光对象")]
    public Light redLight;
    public Light yellowLight;
    public Light greenLight;

    // 私有变量
    private float timer;
    private bool initialized;
    private LightState previousState;

    // 缓存材质引用，避免重复赋值
    private Material currentMaterial;

    // 缓存组件
    private Transform tr;
    private Vector3 cachedPosition;
    private bool hasLights;

    void Start()
    {
        tr = transform;
        cachedPosition = tr.position;
        previousState = currentState;
        hasLights = redLight != null || yellowLight != null || greenLight != null;

        if (!controlledByIntersection)
        {
            timer = GetCurrentStateDuration();
            initialized = true;
        }

        UpdateLightVisual();
    }

    void Update()
    {
        if (!controlledByIntersection && initialized)
        {
            timer -= Time.deltaTime;

            if (timer <= 0)
            {
                SwitchToNextState();
                timer = GetCurrentStateDuration();
            }
        }
    }

    void SwitchToNextState()
    {
        switch (currentState)
        {
            case LightState.Green:
                currentState = LightState.Yellow;
                break;
            case LightState.Yellow:
                currentState = LightState.Red;
                break;
            case LightState.Red:
                currentState = LightState.Green;
                break;
        }

        UpdateLightVisual();
    }

    public void SetState(LightState newState)
    {
        if (currentState == newState) return;

        currentState = newState;
        UpdateLightVisual();
    }

    void UpdateLightVisual()
    {
        // 只在状态改变时更新
        if (currentState == previousState && currentMaterial != null) return;

        // 更新材质（避免重复赋值）
        if (lightRenderer != null)
        {
            Material targetMaterial = GetMaterialForState(currentState);

            if (currentMaterial != targetMaterial)
            {
                lightRenderer.material = targetMaterial;
                currentMaterial = targetMaterial;
            }
        }

        // 更新灯光对象（仅在有灯光时执行）
        if (hasLights)
        {
            UpdateLightObjects();
        }

        previousState = currentState;
    }

    Material GetMaterialForState(LightState state)
    {
        switch (state)
        {
            case LightState.Red: return redMaterial;
            case LightState.Yellow: return yellowMaterial;
            case LightState.Green: return greenMaterial;
            default: return redMaterial;
        }
    }

    void UpdateLightObjects()
    {
        bool isRed = currentState == LightState.Red;
        bool isYellow = currentState == LightState.Yellow;
        bool isGreen = currentState == LightState.Green;

        if (redLight != null && redLight.enabled != isRed)
            redLight.enabled = isRed;

        if (yellowLight != null && yellowLight.enabled != isYellow)
            yellowLight.enabled = isYellow;

        if (greenLight != null && greenLight.enabled != isGreen)
            greenLight.enabled = isGreen;
    }

    float GetCurrentStateDuration()
    {
        switch (currentState)
        {
            case LightState.Red: return redDuration;
            case LightState.Yellow: return yellowDuration;
            case LightState.Green: return greenDuration;
            default: return redDuration;
        }
    }

    public bool CanPass()
    {
        return currentState == LightState.Green;
    }

    private void OnDrawGizmos()
    {
        Color gizmoColor;
        switch (currentState)
        {
            case LightState.Red:
                gizmoColor = Color.red;
                break;
            case LightState.Yellow:
                gizmoColor = Color.yellow;
                break;
            case LightState.Green:
                gizmoColor = Color.green;
                break;
            default:
                gizmoColor = Color.white;
                break;
        }

        Gizmos.color = gizmoColor;

        Vector3 pos = cachedPosition.sqrMagnitude > 0 ? cachedPosition : transform.position;
        Gizmos.DrawWireSphere(pos, 1f);

#if UNITY_EDITOR
        if (controlledByIntersection)
        {
            UnityEditor.Handles.Label(pos + Vector3.up * 1.5f, "路口控制");
        }
#endif
    }
}