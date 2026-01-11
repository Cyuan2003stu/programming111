using UnityEngine;

public class follow1 : MonoBehaviour
{
    [Header("References")]
    public Animator animator;          // 人物 Animator
    public Transform bikeTransform;    // 自行车（实际在移动的物体）

    [Header("Tuning")]
    public float moveThreshold = 0.05f;     // 进入骑行（米/秒）建议 0.05~0.2
    public float releaseThreshold = 0.02f;  // 回待机（米/秒）建议 0.02~0.1
    public float smooth = 0.08f;            // 平滑时间
    public bool horizontalOnly = true;      // 忽略Y，防止抖动

    Vector3 _lastPos;
    float _speed;
    float _vel;
    bool _moving;

    void Reset()
    {
        animator = GetComponent<Animator>();
    }

    void Start()
    {
        if (bikeTransform) _lastPos = bikeTransform.position;
        if (animator) animator.SetFloat("Speed", 0f);
    }

    void FixedUpdate()
    {
        if (!animator || !bikeTransform) return;

        Vector3 cur = bikeTransform.position;
        Vector3 delta = cur - _lastPos;
        _lastPos = cur;

        if (horizontalOnly) delta.y = 0f;

        // ✅ 真实速度：位移/时间
        float rawSpeed = delta.magnitude / Time.fixedDeltaTime;

        // ✅ 双阈值防抖
        if (!_moving && rawSpeed > moveThreshold) _moving = true;
        else if (_moving && rawSpeed < releaseThreshold) _moving = false;

        float target = _moving ? rawSpeed : 0f;

        // ✅ 平滑 + 强制归零
        _speed = Mathf.SmoothDamp(_speed, target, ref _vel, smooth);
        if (!_moving && _speed < 0.001f) _speed = 0f;

        animator.SetFloat("Speed", _speed);
    }
}
