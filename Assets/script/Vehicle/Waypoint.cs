using UnityEngine;

public class Waypoint : MonoBehaviour
{
    [Header("基础设置")]
    public Waypoint[] nextWaypoints;
    public float speedLimit = 10f;
    public Waypoint previousWaypoint;

    [Header("车道信息")]
    public LaneType laneType = LaneType.Straight;
    public int laneIndex = 0;
    public int totalLanes = 1;

    [Header("路口信息")]
    public bool isIntersection = false;
    public IntersectionType intersectionType = IntersectionType.None;

    public enum LaneType
    {
        LeftTurn,
        Straight,
        RightTurn,
        LeftOrStraight,
        StraightOrRight,
        All
    }

    public enum IntersectionType
    {
        None,
        TJunction,
        CrossRoad
    }

    /// <summary>
    /// 获取车辆实际的行驶方向（从上一个waypoint指向当前waypoint）
    /// </summary>
    public Vector3 GetApproachDirection()
    {
        if (previousWaypoint != null)
        {
            return (transform.position - previousWaypoint.transform.position).normalized;
        }
        // 如果没有上一个waypoint，使用forward（仅用于初始化）
        return transform.forward;
    }

    public Waypoint GetNextWaypoint(TurnDirection desiredDirection)
    {
        if (nextWaypoints == null || nextWaypoints.Length == 0)
            return null;

        if (!isIntersection)
        {
            return nextWaypoints[0];
        }

        if (CanTurn(desiredDirection))
        {
            foreach (var wp in nextWaypoints)
            {
                if (wp != null && IsDirectionMatch(wp, desiredDirection))
                {
                    return wp;
                }
            }
        }

        return GetRandomAllowedWaypoint();
    }

    public bool CanTurn(TurnDirection direction)
    {
        switch (laneType)
        {
            case LaneType.LeftTurn:
                return direction == TurnDirection.Left;
            case LaneType.Straight:
                return direction == TurnDirection.Straight;
            case LaneType.RightTurn:
                return direction == TurnDirection.Right;
            case LaneType.LeftOrStraight:
                return direction == TurnDirection.Left || direction == TurnDirection.Straight;
            case LaneType.StraightOrRight:
                return direction == TurnDirection.Straight || direction == TurnDirection.Right;
            case LaneType.All:
                return true;
            default:
                return false;
        }
    }

    public Waypoint GetRandomAllowedWaypoint()
    {
        if (nextWaypoints == null || nextWaypoints.Length == 0)
            return null;

        System.Collections.Generic.List<Waypoint> allowed = new System.Collections.Generic.List<Waypoint>();

        foreach (var direction in System.Enum.GetValues(typeof(TurnDirection)))
        {
            TurnDirection dir = (TurnDirection)direction;

            if (CanTurn(dir))
            {
                foreach (var wp in nextWaypoints)
                {
                    if (wp != null)
                    {
                        bool matches = IsDirectionMatch(wp, dir);

                        if (matches && !allowed.Contains(wp))
                        {
                            allowed.Add(wp);
                        }
                    }
                }
            }
        }

        if (allowed.Count > 0)
        {
            int randomIndex = Random.Range(0, allowed.Count);
            return allowed[randomIndex];
        }

        // 如果没有匹配的，真正随机选择
        Debug.LogWarning($"{name}: 没有符合规则的waypoint，从所有选项中随机选择");
        if (nextWaypoints.Length > 0)
        {
            int randomIndex = Random.Range(0, nextWaypoints.Length);
            return nextWaypoints[randomIndex];
        }

        return null;
    }

    /// <summary>
    /// ✅ 修复：判断waypoint是否匹配期望的方向（使用实际行驶方向）
    /// </summary>
    bool IsDirectionMatch(Waypoint targetWp, TurnDirection direction)
    {
        // ✅ 关键修复：使用车辆的实际行驶方向，而不是waypoint的transform.forward
        Vector3 referenceDir = GetApproachDirection();
        Vector3 toTarget = (targetWp.transform.position - transform.position).normalized;

        float angle = Vector3.SignedAngle(referenceDir, toTarget, Vector3.up);

#if UNITY_EDITOR
        // 可选：调试信息
        // Debug.Log($"[{name}] → {targetWp.name}: angle={angle:F1}°, checking={direction}");
#endif

        switch (direction)
        {
            case TurnDirection.Left:
                // 左转：角度在 45° 到 135° 之间
                return angle > 45f && angle < 135f;

            case TurnDirection.Right:
                // 右转：角度在 -45° 到 -135° 之间
                return angle < -45f && angle > -135f;

            case TurnDirection.Straight:
                // 直行：角度在 -45° 到 45° 之间
                return Mathf.Abs(angle) < 45f;

            default:
                return false;
        }
    }

    public Waypoint GetAdjacentLaneWaypoint(int direction)
    {
        return null;
    }

    protected virtual void OnDrawGizmos()
    {
        Gizmos.color = GetLaneColor();
        Gizmos.DrawWireSphere(transform.position, 0.5f);

        if (nextWaypoints != null)
        {
            foreach (var next in nextWaypoints)
            {
                if (next != null)
                {
                    Gizmos.color = isIntersection ? Color.yellow : Color.green;
                    Gizmos.DrawLine(transform.position, next.transform.position);

                    Vector3 direction = (next.transform.position - transform.position).normalized;
                    Vector3 arrowPos = transform.position + direction *
                        Vector3.Distance(transform.position, next.transform.position) * 0.5f;
                    DrawArrow(arrowPos, direction);
                }
            }
        }

        // 绘制行驶方向箭头（从上一个waypoint来的方向）
        if (previousWaypoint != null)
        {
            Gizmos.color = Color.magenta;
            Vector3 approachDir = GetApproachDirection();
            Gizmos.DrawRay(transform.position, approachDir * 2f);
        }

#if UNITY_EDITOR
        UnityEditor.Handles.Label(transform.position + Vector3.up * 2f,
            $"Lane {laneIndex}\n{laneType}");
#endif
    }

    Color GetLaneColor()
    {
        switch (laneType)
        {
            case LaneType.LeftTurn: return Color.cyan;
            case LaneType.Straight: return Color.green;
            case LaneType.RightTurn: return Color.magenta;
            case LaneType.LeftOrStraight: return Color.blue;
            case LaneType.StraightOrRight: return Color.yellow;
            case LaneType.All: return Color.white;
            default: return Color.gray;
        }
    }

    void DrawArrow(Vector3 pos, Vector3 direction)
    {
        Vector3 right = Quaternion.Euler(0, 30, 0) * direction;
        Vector3 left = Quaternion.Euler(0, -30, 0) * direction;

        Gizmos.DrawRay(pos, -right * 0.5f);
        Gizmos.DrawRay(pos, -left * 0.5f);
    }
}

public enum TurnDirection
{
    Left,
    Straight,
    Right
}