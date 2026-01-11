using UnityEngine;

public class SimpleBikeSteerAndLean : MonoBehaviour
{
    [Header("Who actually moves")]
    public Transform moveRoot;          // 真正移动/前进的根（通常就是本物体）

    [Header("Visual only (bike + rider mesh)")]
    public Transform visualRoot;        // 只倾斜这一层（MODEL 或 车+人父物体）

    [Header("Move")]
    public float moveSpeed = 6f;
    public float turnSpeed = 120f;

    [Header("Lean")]
    public float maxLeanAngle = 15f;
    public float leanSmooth = 10f;

    [Header("Animation (optional)")]
    public Animator riderAnimator;
    public float movingThreshold = 0.01f;

    float currentLean = 0f;
    Quaternion visualBaseLocalRot;

    void Awake()
    {
        if (!moveRoot) moveRoot = transform;
        if (!visualRoot) visualRoot = transform;

        visualBaseLocalRot = visualRoot.localRotation;
    }

    void Update()
    {
        float dt = Time.deltaTime;

        bool w = Input.GetKey(KeyCode.W);

        float steer = 0f;
        if (Input.GetKey(KeyCode.A)) steer -= 1f;
        if (Input.GetKey(KeyCode.D)) steer += 1f;

        float speed = w ? moveSpeed : 0f;
        bool isMoving = speed > movingThreshold;

        // 前进
        if (isMoving)
        {
            moveRoot.position += moveRoot.forward * speed * dt;

            // 转向：只有在前进时才转
            if (Mathf.Abs(steer) > 0.01f)
                moveRoot.Rotate(Vector3.up, steer * turnSpeed * dt, Space.World);
        }

        // 倾斜：只倾斜视觉层
        float targetLean = isMoving ? -steer * maxLeanAngle : 0f;
        currentLean = Mathf.Lerp(currentLean, targetLean, 1f - Mathf.Exp(-leanSmooth * dt));
        visualRoot.localRotation = visualBaseLocalRot * Quaternion.Euler(0f, 0f, currentLean);

        // 动画：车动才播放，停就暂停
        if (riderAnimator) riderAnimator.speed = isMoving ? 1f : 0f;
    }
}
