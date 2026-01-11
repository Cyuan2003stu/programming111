using UnityEngine;

public class TrafficLightWaypoint : Waypoint
{
    [Header("红绿灯设置")]
    public TrafficLight trafficLight; // 关联的红绿灯
    public float stopDistance = 2f; // 停车线距离waypoint的距离

    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();

        // 绘制停车线
        Gizmos.color = Color.red;
        Vector3 stopPosition = transform.position - transform.forward * stopDistance;

        // 根据车道数量绘制不同长度的停车线
        float lineWidth = totalLanes * 3f; // 每条车道3米宽
        Gizmos.DrawLine(
            stopPosition - transform.right * (lineWidth * 0.5f),
            stopPosition + transform.right * (lineWidth * 0.5f)
        );

        // 绘制与红绿灯的连接线
        if (trafficLight != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, trafficLight.transform.position);

            // 显示红绿灯状态
#if UNITY_EDITOR
            Color lightColor = Color.white;
            if (trafficLight.currentState == TrafficLight.LightState.Red)
                lightColor = Color.red;
            else if (trafficLight.currentState == TrafficLight.LightState.Yellow)
                lightColor = Color.yellow;
            else if (trafficLight.currentState == TrafficLight.LightState.Green)
                lightColor = Color.green;

            Gizmos.color = lightColor;
            Gizmos.DrawWireSphere(trafficLight.transform.position, 1.5f);
#endif
        }
    }
}