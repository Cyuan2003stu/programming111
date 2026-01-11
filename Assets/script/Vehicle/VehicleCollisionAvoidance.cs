using UnityEngine;

/// <summary>
/// 车辆碰撞避免系统 - 检测周围车辆并保持安全距离
/// 这是一个独立组件，可以添加到任何车辆上
/// </summary>
public class VehicleCollisionAvoidance : MonoBehaviour
{
    [Header("检测设置")]
    [Tooltip("周围检测范围（米）")]
    [Range(1f, 20f)]
    public float detectionRadius = 8f;

    [Tooltip("安全距离（米）- 与其他车辆保持的最小距离")]
    [Range(1f, 10f)]
    public float safeDistance = 3f;

    [Tooltip("侧面检测距离（米）")]
    [Range(1f, 15f)]
    public float sideDetectionDistance = 5f;

    [Tooltip("车辆检测层")]
    public LayerMask vehicleLayer;

    [Header("避让行为")]
    [Tooltip("速度降低因子（0-1，数值越大减速越快）")]
    [Range(0f, 1f)]
    public float speedReductionFactor = 0.8f;

    [Tooltip("紧急停车距离（米）")]
    [Range(0.5f, 3f)]
    public float emergencyStopDistance = 1.5f;

    [Tooltip("转向避让强度")]
    [Range(0f, 5f)]
    public float avoidanceStrength = 2f;

    [Tooltip("启用侧面避让")]
    public bool enableSideAvoidance = true;

    [Header("调试")]
    [Tooltip("显示检测范围")]
    public bool showDebugGizmos = true;

    [Tooltip("显示调试日志")]
    public bool showDebugLogs = false;

    // 检测结果
    private Collider[] nearbyVehicles = new Collider[20];
    private int nearbyCount = 0;

    // 避让信息
    private Vector3 avoidanceDirection = Vector3.zero;
    private float currentSpeedMultiplier = 1f;
    private bool isEmergencyStop = false;

    // 缓存
    private Transform cachedTransform;
    private VehicleAI vehicleAI;

    // 检测区域信息
    private struct DetectionZone
    {
        public string name;
        public Vector3 direction;
        public float distance;
        public float angle;

        public DetectionZone(string n, Vector3 dir, float dist, float ang)
        {
            name = n;
            direction = dir;
            distance = dist;
            angle = ang;
        }
    }

    void Start()
    {
        cachedTransform = transform;
        vehicleAI = GetComponent<VehicleAI>();

        // 验证配置
        if (vehicleLayer.value == 0)
        {
            Debug.LogWarning($"[{name}] CollisionAvoidance: Vehicle Layer 未设置！");
        }

        if (vehicleAI == null)
        {
            Debug.LogWarning($"[{name}] CollisionAvoidance: 未找到 VehicleAI 组件，" +
                           "某些功能可能受限。");
        }
    }

    void FixedUpdate()
    {
        DetectNearbyVehicles();
        CalculateAvoidanceResponse();
    }

    /// <summary>
    /// 检测周围的车辆
    /// </summary>
    void DetectNearbyVehicles()
    {
        Vector3 position = cachedTransform.position;

        // 使用 OverlapSphereNonAlloc 检测周围车辆
        nearbyCount = Physics.OverlapSphereNonAlloc(
            position,
            detectionRadius,
            nearbyVehicles,
            vehicleLayer
        );

        if (showDebugLogs && Time.frameCount % 60 == 0)
        {
            Debug.Log($"[{name}] 检测到 {nearbyCount} 辆周围车辆");
        }
    }

    /// <summary>
    /// 计算避让响应
    /// </summary>
    void CalculateAvoidanceResponse()
    {
        // 重置避让信息
        avoidanceDirection = Vector3.zero;
        currentSpeedMultiplier = 1f;
        isEmergencyStop = false;

        if (nearbyCount == 0)
        {
            return;
        }

        Vector3 myPosition = cachedTransform.position;
        Vector3 myForward = cachedTransform.forward;
        Vector3 myRight = cachedTransform.right;

        Vector3 totalAvoidanceForce = Vector3.zero;
        float closestDistance = float.MaxValue;
        int threatsCount = 0;

        // 分析每辆附近的车辆
        for (int i = 0; i < nearbyCount; i++)
        {
            if (nearbyVehicles[i] == null) continue;

            GameObject otherVehicle = nearbyVehicles[i].gameObject;

            // 跳过自己
            if (otherVehicle == gameObject ||
                otherVehicle.transform.root == cachedTransform.root)
            {
                continue;
            }

            Vector3 otherPosition = otherVehicle.transform.position;
            Vector3 toOther = otherPosition - myPosition;
            toOther.y = 0; // 忽略高度差

            float distance = toOther.magnitude;

            if (distance < 0.01f) continue; // 避免除零

            Vector3 toOtherNormalized = toOther / distance;

            // 计算相对角度（判断是前方、侧面还是后方）
            float forwardDot = Vector3.Dot(myForward, toOtherNormalized);
            float rightDot = Vector3.Dot(myRight, toOtherNormalized);

            // 判断威胁等级
            bool isThreat = false;
            float threatLevel = 0f;

            // 前方威胁（最危险）
            if (forwardDot > 0.5f && distance < sideDetectionDistance)
            {
                isThreat = true;
                threatLevel = 1f - (distance / sideDetectionDistance);

                if (distance < emergencyStopDistance)
                {
                    isEmergencyStop = true;
                }

                if (showDebugLogs && Time.frameCount % 60 == 0)
                {
                    Debug.Log($"[{name}] 前方威胁: {otherVehicle.name}, " +
                             $"距离: {distance:F1}m");
                }
            }
            // 侧面威胁
            else if (enableSideAvoidance && Mathf.Abs(rightDot) > 0.7f &&
                     distance < safeDistance)
            {
                isThreat = true;
                threatLevel = 0.5f * (1f - (distance / safeDistance));

                if (showDebugLogs && Time.frameCount % 60 == 0)
                {
                    string side = rightDot > 0 ? "右侧" : "左侧";
                    Debug.Log($"[{name}] {side}威胁: {otherVehicle.name}, " +
                             $"距离: {distance:F1}m");
                }
            }

            // 如果构成威胁，计算避让力
            if (isThreat)
            {
                threatsCount++;

                // 避让方向：远离其他车辆
                Vector3 avoidDirection = -toOtherNormalized;
                avoidDirection.y = 0;

                // 根据威胁等级加权
                totalAvoidanceForce += avoidDirection * threatLevel;

                // 记录最近距离
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                }

                // 绘制调试线
                if (showDebugGizmos)
                {
                    Color debugColor = distance < emergencyStopDistance ?
                        Color.red : Color.yellow;
                    Debug.DrawLine(myPosition, otherPosition, debugColor, 0.1f);
                }
            }
        }

        // 如果有威胁，应用避让
        if (threatsCount > 0)
        {
            // 计算避让方向
            avoidanceDirection = totalAvoidanceForce.normalized;

            // 计算速度降低倍数
            if (isEmergencyStop)
            {
                currentSpeedMultiplier = 0f; // 紧急停车
            }
            else
            {
                // 根据最近距离计算速度倍数
                float distanceRatio = Mathf.Clamp01(
                    (closestDistance - emergencyStopDistance) /
                    (safeDistance - emergencyStopDistance)
                );
                currentSpeedMultiplier = Mathf.Lerp(
                    0.2f,
                    1f,
                    distanceRatio
                ) * speedReductionFactor;
            }

            if (showDebugLogs && Time.frameCount % 30 == 0)
            {
                Debug.Log($"[{name}] 避让中 - 威胁数: {threatsCount}, " +
                         $"最近距离: {closestDistance:F1}m, " +
                         $"速度倍数: {currentSpeedMultiplier:F2}, " +
                         $"紧急停车: {isEmergencyStop}");
            }
        }
    }

    /// <summary>
    /// 获取当前速度倍数（供外部调用）
    /// </summary>
    public float GetSpeedMultiplier()
    {
        return currentSpeedMultiplier;
    }

    /// <summary>
    /// 获取避让方向（供外部调用）
    /// </summary>
    public Vector3 GetAvoidanceDirection()
    {
        return avoidanceDirection;
    }

    /// <summary>
    /// 是否需要紧急停车（供外部调用）
    /// </summary>
    public bool IsEmergencyStopping()
    {
        return isEmergencyStop;
    }

    /// <summary>
    /// 应用避让到 Rigidbody（可选，自动避让模式）
    /// </summary>
    public void ApplyAvoidanceToRigidbody(Rigidbody rb, float currentSpeed)
    {
        if (rb == null) return;

        // 如果有避让方向，轻微调整朝向
        if (avoidanceDirection.sqrMagnitude > 0.01f)
        {
            Vector3 desiredDirection = cachedTransform.forward +
                                      avoidanceDirection * avoidanceStrength * Time.fixedDeltaTime;
            desiredDirection.y = 0;

            if (desiredDirection.sqrMagnitude > 0.01f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(desiredDirection);
                rb.rotation = Quaternion.Slerp(
                    rb.rotation,
                    targetRotation,
                    5f * Time.fixedDeltaTime
                );
            }
        }

        // 应用速度调整（如果有 VehicleAI 则不自动调整，让 VehicleAI 处理）
        if (vehicleAI == null)
        {
            float adjustedSpeed = currentSpeed * currentSpeedMultiplier;
            rb.MovePosition(rb.position +
                          cachedTransform.forward * adjustedSpeed * Time.fixedDeltaTime);
        }
    }

    /// <summary>
    /// 检测特定方向是否安全
    /// </summary>
    public bool IsDirectionSafe(Vector3 direction, float checkDistance)
    {
        direction.y = 0;
        direction.Normalize();

        Vector3 startPos = cachedTransform.position + Vector3.up * 0.5f;

        // 使用 SphereCast 检测方向
        RaycastHit hit;
        if (Physics.SphereCast(
            startPos,
            0.5f, // 半径
            direction,
            out hit,
            checkDistance,
            vehicleLayer))
        {
            // 检测到车辆
            if (hit.collider.gameObject != gameObject)
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// 获取最近车辆的信息（供调试）
    /// </summary>
    public GameObject GetClosestVehicle(out float distance)
    {
        distance = float.MaxValue;
        GameObject closest = null;

        Vector3 myPosition = cachedTransform.position;

        for (int i = 0; i < nearbyCount; i++)
        {
            if (nearbyVehicles[i] == null) continue;

            GameObject other = nearbyVehicles[i].gameObject;

            if (other == gameObject || other.transform.root == cachedTransform.root)
                continue;

            float dist = Vector3.Distance(myPosition, other.transform.position);

            if (dist < distance)
            {
                distance = dist;
                closest = other;
            }
        }

        return closest;
    }

    #region Gizmos 调试可视化

    private void OnDrawGizmos()
    {
        if (!showDebugGizmos) return;

        Transform t = transform;
        Vector3 pos = t.position;

        // 绘制检测半径
        Gizmos.color = new Color(0, 1, 1, 0.2f); // 半透明青色
        DrawWireCircle(pos, detectionRadius, 32);

        // 绘制安全距离
        Gizmos.color = new Color(1, 1, 0, 0.3f); // 半透明黄色
        DrawWireCircle(pos, safeDistance, 24);

        // 绘制紧急停车距离
        Gizmos.color = new Color(1, 0, 0, 0.4f); // 半透明红色
        DrawWireCircle(pos, emergencyStopDistance, 16);
    }

    private void OnDrawGizmosSelected()
    {
        if (!showDebugGizmos) return;

        Transform t = transform;
        Vector3 pos = t.position;
        Vector3 forward = t.forward;
        Vector3 right = t.right;

        // 绘制前方检测区域
        Gizmos.color = Color.green;
        Vector3 frontLeft = pos + forward * sideDetectionDistance + right * 1f;
        Vector3 frontRight = pos + forward * sideDetectionDistance - right * 1f;
        Gizmos.DrawLine(pos + right * 1f, frontLeft);
        Gizmos.DrawLine(pos - right * 1f, frontRight);
        Gizmos.DrawLine(frontLeft, frontRight);

        // 绘制当前避让方向
        if (Application.isPlaying && avoidanceDirection.sqrMagnitude > 0.01f)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawRay(pos, avoidanceDirection * 3f);
            Gizmos.DrawWireSphere(pos + avoidanceDirection * 3f, 0.3f);
        }

        // 显示信息
#if UNITY_EDITOR
        if (Application.isPlaying)
        {
            string info = $"检测到: {nearbyCount} 辆\n" +
                         $"速度倍数: {currentSpeedMultiplier:F2}\n" +
                         $"紧急: {isEmergencyStop}";
            UnityEditor.Handles.Label(pos + Vector3.up * 2.5f, info);
        }
#endif
    }

    // 辅助方法：绘制水平圆圈
    void DrawWireCircle(Vector3 center, float radius, int segments)
    {
        float angleStep = 360f / segments;
        Vector3 prevPoint = center + new Vector3(radius, 0, 0);

        for (int i = 1; i <= segments; i++)
        {
            float angle = i * angleStep * Mathf.Deg2Rad;
            Vector3 newPoint = center + new Vector3(
                Mathf.Cos(angle) * radius,
                0,
                Mathf.Sin(angle) * radius
            );
            Gizmos.DrawLine(prevPoint, newPoint);
            prevPoint = newPoint;
        }
    }

    #endregion
}