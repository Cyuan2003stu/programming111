using UnityEngine;

/// <summary>
/// 简化版车辆AI控制脚本 - 负责移动、转向、车间距保持、红绿灯识别
/// </summary>
public class VehicleAI : MonoBehaviour
{
    [Header("移动设置")]
    public float maxSpeed = 10f;
    public float acceleration = 3f;
    public float brakeForce = 8f;
    public float rotationSpeed = 5f;
    public float waypointReachDistance = 2f;

    [Header("路径设置")]
    public Waypoint currentWaypoint;

    [Header("车间距设置")]
    public float desiredFollowDistance = 10f;
    public float emergencyStopDistance = 3f;
    public float detectionRange = 30f;
    public LayerMask vehicleLayer;
    public float rayStartOffset = 2f;

    [Header("红绿灯设置")]
    public float trafficLightCheckDistance = 20f;
    public float trafficLightStopDistance = 2f;

    [Header("路口决策")]
    public TurnDirection preferredDirection = TurnDirection.Straight;
    public bool randomizeDirection = true;

    // 私有变量
    private float currentSpeed = 0f;
    private Rigidbody rb;
    private BoxCollider vehicleCollider;
    private Transform cachedTransform;
    private VehicleCollisionAvoidance collisionAvoidance;

    // 性能优化：缓存常用值
    private Vector3 cachedForward;
    private float waypointReachDistanceSqr;
    private float rayOriginOffset;
    private readonly RaycastHit[] hitBuffer = new RaycastHit[10];

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        vehicleCollider = GetComponent<BoxCollider>();
        cachedTransform = transform;
        collisionAvoidance = GetComponent<VehicleCollisionAvoidance>();

        if (rb == null || vehicleCollider == null)
        {
            Debug.LogError($"[{name}] 缺少必需组件！需要 Rigidbody 和 BoxCollider");
            enabled = false;
            return;
        }

        rb.isKinematic = false;
        rb.useGravity = true;
        rb.constraints = RigidbodyConstraints.FreezeRotationX |
                        RigidbodyConstraints.FreezeRotationZ;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        waypointReachDistanceSqr = waypointReachDistance * waypointReachDistance;
        rayOriginOffset = vehicleCollider.size.z * 0.5f;
        int vehicleLayerIndex = LayerMask.NameToLayer("Vehicle");

        if (vehicleLayer.value == 0)
        {
            Debug.LogWarning($"[{name}] Vehicle Layer 没有设置！");
        }

        if (currentWaypoint != null)
        {
            SkipNearbyWaypoints();
        }

        if (randomizeDirection)
        {
            preferredDirection = (TurnDirection)Random.Range(0, 3);
        }
    }

    void FixedUpdate()
    {
        if (currentWaypoint == null || rb == null) return;

        cachedForward = cachedTransform.forward;

        float distanceToFrontVehicle = DetectFrontVehicle();
        float distanceToTrafficLight = CheckTrafficLight();
        float targetSpeed = CalculateTargetSpeed(distanceToFrontVehicle, distanceToTrafficLight);

        AdjustSpeed(targetSpeed);
        MoveAndRotate();
    }

    float DetectFrontVehicle()
    {
        Vector3 rayOrigin = cachedTransform.position;
        rayOrigin.y += 0.5f;
        rayOrigin += cachedForward * (rayOriginOffset + rayStartOffset);

        int hitCount = Physics.RaycastNonAlloc(
            rayOrigin,
            cachedForward,
            hitBuffer,
            detectionRange,
            vehicleLayer
        );

        for (int i = 0; i < hitCount; i++)
        {
            GameObject hitObj = hitBuffer[i].collider.gameObject;

            if (hitObj != gameObject && hitObj.transform.root != cachedTransform.root)
            {
                return hitBuffer[i].distance;
            }
        }

        return float.MaxValue;
    }

    float CheckTrafficLight()
    {
        TrafficLightWaypoint tlWaypoint = currentWaypoint as TrafficLightWaypoint;

        if (tlWaypoint != null && tlWaypoint.trafficLight != null)
        {
            TrafficLight light = tlWaypoint.trafficLight;

            if (!light.CanPass())
            {
                Vector3 diff = tlWaypoint.transform.position - cachedTransform.position;
                float distanceSqr = diff.x * diff.x + diff.z * diff.z;
                float distance = Mathf.Sqrt(distanceSqr);

                if (distance < trafficLightCheckDistance)
                {
                    return Mathf.Max(0, distance - tlWaypoint.stopDistance);
                }
            }
        }

        return float.MaxValue;
    }

    float CalculateTargetSpeed(float distanceToFrontVehicle, float distanceToTrafficLight)
    {
        float targetSpeed = maxSpeed;

        // 车距控制
        if (distanceToFrontVehicle < detectionRange)
        {
            if (distanceToFrontVehicle < emergencyStopDistance)
            {
                targetSpeed = 0f;
            }
            else if (distanceToFrontVehicle < desiredFollowDistance)
            {
                float ratio = (distanceToFrontVehicle - emergencyStopDistance) /
                             (desiredFollowDistance - emergencyStopDistance);
                targetSpeed = maxSpeed * ratio * 0.8f;
            }
            else
            {
                targetSpeed = maxSpeed * 0.9f;
            }
        }

        // 红绿灯控制
        if (distanceToTrafficLight < trafficLightCheckDistance)
        {
            if (distanceToTrafficLight < trafficLightStopDistance)
            {
                return 0f;
            }

            float ratio = distanceToTrafficLight / trafficLightCheckDistance;
            float trafficLightSpeed = maxSpeed * ratio;
            targetSpeed = Mathf.Min(targetSpeed, trafficLightSpeed);
        }

        // 碰撞避免控制
        if (collisionAvoidance != null)
        {
            if (collisionAvoidance.IsEmergencyStopping())
            {
                return 0f;
            }
            targetSpeed *= collisionAvoidance.GetSpeedMultiplier();
        }

        return targetSpeed;
    }

    void AdjustSpeed(float targetSpeed)
    {
        float delta = (targetSpeed < currentSpeed ? brakeForce : acceleration) * Time.fixedDeltaTime;
        currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, delta);
        currentSpeed = Mathf.Max(0, currentSpeed);
    }

    void MoveAndRotate()
    {
        if (currentWaypoint == null) return;

        Vector3 targetPosition = currentWaypoint.transform.position;
        Vector3 direction = targetPosition - cachedTransform.position;
        direction.y = 0;

        float distanceSqr = direction.sqrMagnitude;

        // 到达waypoint
        if (distanceSqr < waypointReachDistanceSqr)
        {
            if (currentWaypoint.isIntersection)
            {
                SelectNextWaypointAtIntersection();
                return;
            }

            if (currentWaypoint.nextWaypoints != null &&
                currentWaypoint.nextWaypoints.Length > 0)
            {
                Waypoint nextWp = currentWaypoint.nextWaypoints[0];

                if (nextWp != null)
                {
                    nextWp.previousWaypoint = currentWaypoint;
                    currentWaypoint = nextWp;
                }
            }

            return;
        }

        // 转向
        if (distanceSqr > 0.0001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            rb.rotation = Quaternion.Slerp(
                rb.rotation,
                targetRotation,
                rotationSpeed * Time.fixedDeltaTime
            );
        }

        // 移动
        rb.MovePosition(rb.position + cachedForward * currentSpeed * Time.fixedDeltaTime);
    }

    void SelectNextWaypointAtIntersection()
    {
        if (currentWaypoint.nextWaypoints == null ||
            currentWaypoint.nextWaypoints.Length == 0)
        {
            return;
        }

        Waypoint nextWp = randomizeDirection ?
            currentWaypoint.GetRandomAllowedWaypoint() :
            currentWaypoint.GetNextWaypoint(preferredDirection);

        if (nextWp == null)
        {
            nextWp = currentWaypoint.GetRandomAllowedWaypoint();
        }

        if (nextWp != null)
        {
            nextWp.previousWaypoint = currentWaypoint;
            currentWaypoint = nextWp;
        }
    }

    void SkipNearbyWaypoints()
    {
        Vector3 pos = cachedTransform.position;

        while (currentWaypoint != null)
        {
            Vector3 diff = currentWaypoint.transform.position - pos;
            float distanceSqr = diff.x * diff.x + diff.y * diff.y + diff.z * diff.z;

            if (distanceSqr >= waypointReachDistanceSqr) break;

            if (currentWaypoint.nextWaypoints != null &&
                currentWaypoint.nextWaypoints.Length > 0)
            {
                Waypoint nextWp = currentWaypoint.nextWaypoints[0];
                if (nextWp != null)
                {
                    nextWp.previousWaypoint = currentWaypoint;
                    currentWaypoint = nextWp;
                }
                else break;
            }
            else break;
        }
    }
}