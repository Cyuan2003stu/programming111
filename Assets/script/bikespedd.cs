using UnityEngine;

public class BikeMoveSmooth : MonoBehaviour
{
    [Header("速度参数")]
    public float maxSpeed = 8f;        // 最大速度
    public float acceleration = 5f;    // 起步加速度（越大起步越快）
    public float deceleration = 6f;    // 减速加速度（越大减速越快）

    [Header("方向设置")]
    public Transform forwardReference; // 自行车前进方向（通常就是车本身的transform）

    private float currentSpeed = 0f;   // 当前速度

    void Update()
    {
        // 你自己的输入，可以换成手柄/按键
        float input = Input.GetAxisRaw("Vertical"); // W/S 或 ↑/↓

        // 1. 计算目标速度（只示例前进，input > 0 才前进）
        float targetSpeed = 0f;
        if (input > 0f)
        {
            targetSpeed = maxSpeed;
        }
        else
        {
            targetSpeed = 0f; // 松开就慢慢停下
        }

        // 2. 缓动 currentSpeed 到 targetSpeed
        if (targetSpeed > currentSpeed)
        {
            // 加速
            currentSpeed = Mathf.MoveTowards(
                currentSpeed,
                targetSpeed,
                acceleration * Time.deltaTime
            );
        }
        else
        {
            // 减速
            currentSpeed = Mathf.MoveTowards(
                currentSpeed,
                targetSpeed,
                deceleration * Time.deltaTime
            );
        }

        // 3. 根据 currentSpeed 移动
        if (forwardReference == null)
        {
            forwardReference = transform; // 没拖就用自己前方
        }

        Vector3 dir = forwardReference.forward;
        transform.position += dir * currentSpeed * Time.deltaTime;
    }
}
