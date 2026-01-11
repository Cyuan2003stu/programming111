using System.Collections;
using UnityEngine;

/// <summary>
/// 自行车 + 骑手 撞击摔倒 + 恢复 的控制脚本
/// 挂在【自行车根物体】上
/// </summary>
public class BikeCrashController : MonoBehaviour
{
    [Header("引用")]
    public Rigidbody bikeRb;          // 自行车的刚体（挂在车架根节点）
    public Rigidbody riderRb;         // 骑手的刚体（平时 isKinematic = true）
    public Transform riderRoot;       // 骑手根节点，用来推的方向
    public Animator riderAnimator;    // 骑手 Animator（有骑行/待机动画）

    [Header("摔倒设置")]
    public float minCrashSpeed = 3f;  // 触发摔倒的最小碰撞速度
    public float fallForce = 6f;      // 把人甩出去的力度
    public float recoverDelay = 2f;   // 摔倒后等待多少秒恢复
    public string obstacleTag = "Obstacle";   // 障碍物的 Tag（自己在 Inspector 里设置）

    [Header("可选：简单前进测试")]
    public bool simpleMoveTest = true;    // 勾上可以用 W/S 做一个简单前进测试
    public float moveSpeed = 5f;          // 简单测试时的移动速度

    // 内部状态
    private bool isFallen = false;

    // 记录初始位姿，用于恢复
    private Vector3 bikeStartPos;
    private Quaternion bikeStartRot;
    private Vector3 riderStartLocalPos;
    private Quaternion riderStartLocalRot;

    private void Start()
    {
        // 记录初始姿态
        bikeStartPos = transform.position;
        bikeStartRot = transform.rotation;

        if (riderRoot != null)
        {
            riderStartLocalPos = riderRoot.localPosition;
            riderStartLocalRot = riderRoot.localRotation;
        }

        // 默认：骑手刚体不参与物理，只跟随动画
        if (riderRb != null)
        {
            riderRb.isKinematic = true;
        }
    }

    private void Update()
    {
        // 只是为了你方便测试：按 W/S 控制简单前进
        if (!isFallen && simpleMoveTest && bikeRb != null)
        {
            float input = Input.GetAxis("Vertical"); // W/S 或 ↑/↓
            Vector3 vel = transform.forward * (input * moveSpeed);
            bikeRb.linearVelocity = new Vector3(vel.x, bikeRb.linearVelocity.y, vel.z);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (isFallen) return;

        // 只跟特定 Tag 的物体碰撞才算障碍物（比如路沿、车、墙）
        if (!string.IsNullOrEmpty(obstacleTag) &&
            !collision.collider.CompareTag(obstacleTag))
        {
            return;
        }

        // 碰撞强度（相对速度）
        float impactSpeed = collision.relativeVelocity.magnitude;

        if (impactSpeed >= minCrashSpeed)
        {
            Vector3 hitPoint = collision.contacts[0].point;
            TriggerFall(hitPoint);
        }
    }

    /// <summary>
    /// 触发摔倒
    /// </summary>
    private void TriggerFall(Vector3 hitPoint)
    {
        if (isFallen) return;
        isFallen = true;

        // 停止骑行动画
        if (riderAnimator != null)
        {
            riderAnimator.enabled = false;
        }

        // 让骑手刚体参与物理
        if (riderRb != null)
        {
            riderRb.isKinematic = false;

            // 计算被撞方向：从撞击点指向骑手 + 少许向上，让他有一点飞起
            Vector3 pushDir = (riderRoot.position - hitPoint).normalized
                              + Vector3.up * 0.5f;
            pushDir.Normalize();

            riderRb.AddForce(pushDir * fallForce, ForceMode.Impulse);
        }

        // 自行车也受点力，稍微倾倒一下
        if (bikeRb != null)
        {
            bikeRb.constraints = RigidbodyConstraints.None; // 允许旋转
            bikeRb.AddTorque(transform.right * fallForce, ForceMode.Impulse);
        }

        // 开始等待恢复
        StartCoroutine(RecoverRoutine());
    }

    /// <summary>
    /// 摔倒后等待一段时间，再恢复到初始状态
    /// </summary>
    private IEnumerator RecoverRoutine()
    {
        yield return new WaitForSeconds(recoverDelay);

        // 清空当前速度
        if (bikeRb != null)
        {
            bikeRb.linearVelocity = Vector3.zero;
            bikeRb.angularVelocity = Vector3.zero;
        }

        if (riderRb != null)
        {
            riderRb.linearVelocity = Vector3.zero;
            riderRb.angularVelocity = Vector3.zero;
        }

        // 恢复到初始位置/朝向
        transform.SetPositionAndRotation(bikeStartPos, bikeStartRot);

        if (riderRoot != null)
        {
            riderRoot.localPosition = riderStartLocalPos;
            riderRoot.localRotation = riderStartLocalRot;
        }

        // 骑手重新回到“挂在车上”的状态：关闭物理，打开动画
        if (riderRb != null)
        {
            riderRb.isKinematic = true;
        }

        if (riderAnimator != null)
        {
            riderAnimator.enabled = true;
            riderAnimator.SetTrigger("Ride"); // 可选：设一个骑行动画触发
        }

        // 自行车恢复受控（如果你有其他移动脚本，可以在这里重新约束刚体）
        if (bikeRb != null)
        {
            bikeRb.constraints = RigidbodyConstraints.FreezeRotationX |
                                 RigidbodyConstraints.FreezeRotationZ;
        }

        isFallen = false;
    }
}
