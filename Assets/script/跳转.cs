using UnityEngine;

public class RiderPauseByBikeSpeed : MonoBehaviour
{
    [SerializeField] private Animator riderAnimator;
    [SerializeField] private Rigidbody bikeRb; // 自行车真正移动的刚体
    [SerializeField] private float moveThreshold = 0.05f; // 小于这个速度就当停车
    [SerializeField] private float smooth = 12f; // 平滑，避免抖动

    private float current = 0f;

    void Reset()
    {
        riderAnimator = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        if (!riderAnimator || !bikeRb) return;

        Vector3 v = bikeRb.linearVelocity;
        v.y = 0f; // 忽略上下抖动
        float speed = v.magnitude;

        float target = (speed > moveThreshold) ? 1f : 0f; // 1=正常播放，0=暂停冻结
        current = Mathf.Lerp(current, target, 1f - Mathf.Exp(-smooth * Time.deltaTime));
        riderAnimator.speed = current;
    }
}
