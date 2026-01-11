using UnityEngine;

public class LaneChangeWaypoint : Waypoint
{
    [Header("变道设置")]
    public Waypoint leftLaneWaypoint;  // 左侧车道对应的waypoint
    public Waypoint rightLaneWaypoint; // 右侧车道对应的waypoint
    public bool allowLaneChangeToLeft = true;
    public bool allowLaneChangeToRight = true;

    /// <summary>
    /// 获取指定方向的相邻车道waypoint
    /// </summary>
    public Waypoint GetLaneChangeWaypoint(int direction)
    {
        if (direction < 0 && allowLaneChangeToLeft)
        {
            return leftLaneWaypoint;
        }
        else if (direction > 0 && allowLaneChangeToRight)
        {
            return rightLaneWaypoint;
        }

        return null;
    }

    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();

        // 绘制变道连线
        Gizmos.color = Color.cyan;
        if (leftLaneWaypoint != null && allowLaneChangeToLeft)
        {
            Gizmos.DrawLine(transform.position, leftLaneWaypoint.transform.position);
        }

        if (rightLaneWaypoint != null && allowLaneChangeToRight)
        {
            Gizmos.DrawLine(transform.position, rightLaneWaypoint.transform.position);
        }
    }
}