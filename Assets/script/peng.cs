using System.Collections;
using UnityEngine;

public class BikeCrashRespawn : MonoBehaviour
{
    [Header("Refs (drag in Inspector)")]
    public Rigidbody bikeRb;                 // BIKE 的 Rigidbody（根物体上的）
    public Transform bikeRoot;               // 真正移动/传送的根（通常是 BIKE）
    public Transform visualRoot;             // 视觉倾倒的根（比如 MODEL：车+人视觉）
    public Animator riderAnimator;           // 人物 Animator（PEOPLE 上的 Animator）

    [Header("Disable On Crash (optional)")]
    public Behaviour[] disableOnCrash;       // 摔倒时禁用哪些脚本（BikeController等）

    [Header("Crash Trigger")]
    public float crashCooldown = 0.3f;       // 防抖：连续碰撞别反复触发
    public LayerMask obstacleMask;           // 什么算“障碍物”（不要把 Ground 勾进来）
    public float minImpactSpeed = 1.2f;      // 相对碰撞速度阈值（越大越不容易摔）
    public float minRelativeSpeed = 0.6f;    // 再加一道保险（有些碰撞 relativeVelocity 很小）
    public string fallTriggerName = "Fall";  // Animator 里的 Trigger 名字

    [Header("Fall + Respawn")]
    public float fallDuration = 1.5f;        // 摔倒后多久开始复活
    public float respawnRadius = 6f;         // 复活搜索半径
    public int respawnTries = 24;            // 尝试次数
    public float groundRayHeight = 10f;      // 从上往下射线找地
    public LayerMask groundMask;             // 什么算地面（建议只勾 Ground）
    public float extraGroundOffset = 0.05f;  // 额外抬高一点，避免贴地抖动

    [Header("Clearance (avoid spawning inside things)")]
    public float clearanceRadius = 0.4f;     // 检测范围半径
    public float clearanceHeight = 1.7f;     // 检测高度（接近人高）
    public float respawnInvulnerable = 0.6f; // 复活后短暂无敌，避免刚落地又触发

    [Header("Optional: Tip Visual When Crash")]
    public float tipAngle = 75f;             // 视觉倾倒角度
    public float tipSpeed = 6f;              // 倾倒/回正速度

    bool crashed;
    float lastCrashTime;
    Quaternion visualStartRot;

    void Reset()
    {
        // 尽量自动填一些
        bikeRoot = transform;
        bikeRb = GetComponent<Rigidbody>();
    }

    void Awake()
    {
        if (!bikeRoot) bikeRoot = transform;
        if (!bikeRb) bikeRb = bikeRoot.GetComponent<Rigidbody>();
        if (visualRoot) visualStartRot = visualRoot.localRotation;
    }

    void OnEnable()
    {
        if (visualRoot) visualStartRot = visualRoot.localRotation;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (crashed) return;
        if (Time.time - lastCrashTime < crashCooldown) return;

        // 只对 obstacleMask 里的物体触发
        int otherLayerBit = 1 << collision.gameObject.layer;
        if ((obstacleMask.value & otherLayerBit) == 0) return;

        // 碰撞强度判定
        float relSpeed = collision.relativeVelocity.magnitude;
        float mySpeed = bikeRb ? bikeRb.linearVelocity.magnitude : 0f;

        if (relSpeed < minRelativeSpeed && mySpeed < minImpactSpeed) return;

        lastCrashTime = Time.time;
        StartCoroutine(CrashAndRespawn());
    }

    IEnumerator CrashAndRespawn()
    {
        crashed = true;

        // 1) 禁用控制
        SetControlsEnabled(false);

        // 2) 播放人物摔倒动画（Trigger）
        if (riderAnimator)
        {
            riderAnimator.speed = 1f;
            riderAnimator.ResetTrigger(fallTriggerName);
            riderAnimator.SetTrigger(fallTriggerName);
        }

        // 3) 视觉倾倒（只动 visualRoot，不动真正的 bikeRoot 方向，防止“转向失灵/地图跟着歪”）
        if (visualRoot)
        {
            // 目标：绕前进方向倾倒（左右随机或按当前速度方向决定）
            float side = 1f;
            Vector3 v = bikeRb ? bikeRb.linearVelocity : Vector3.forward;
            Vector3 right = bikeRoot.right;
            if (Vector3.Dot(v, right) < 0f) side = -1f; // 速度在左侧就向左倒

            Quaternion target = visualStartRot * Quaternion.Euler(0f, 0f, side * tipAngle);

            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime * tipSpeed;
                visualRoot.localRotation = Quaternion.Slerp(visualRoot.localRotation, target, t);
                yield return null;
            }
        }

        // 4) 等待摔倒时间
        yield return new WaitForSeconds(fallDuration);

        // 5) 找安全复活点
        Vector3 fromPos = bikeRoot.position;
        Quaternion fromRot = Quaternion.Euler(0f, bikeRoot.eulerAngles.y, 0f);

        Vector3 safePos;
        Quaternion safeRot;
        bool ok = TryFindSafeRespawn(fromPos, fromRot, out safePos, out safeRot);

        if (!ok)
        {
            // 找不到就回到原地（但抬高）
            safePos = fromPos + Vector3.up * 0.5f;
            safeRot = fromRot;
        }

        // 6) 传送并重置物理（关键：加高度偏移，避免“生成在地下”）
        TeleportTo(safePos, safeRot);

        // 7) 视觉回正
        if (visualRoot)
        {
            float t2 = 0f;
            while (t2 < 1f)
            {
                t2 += Time.deltaTime * tipSpeed;
                visualRoot.localRotation = Quaternion.Slerp(visualRoot.localRotation, visualStartRot, t2);
                yield return null;
            }
        }

        // 8) 复活后短暂无敌
        yield return new WaitForSeconds(respawnInvulnerable);

        // 9) 恢复控制
        SetControlsEnabled(true);
        crashed = false;
    }

    void SetControlsEnabled(bool enabled)
    {
        if (disableOnCrash != null)
        {
            for (int i = 0; i < disableOnCrash.Length; i++)
            {
                if (disableOnCrash[i]) disableOnCrash[i].enabled = enabled;
            }
        }

        // 保险：摔倒时停住刚体
        if (!enabled && bikeRb)
        {
            bikeRb.linearVelocity = Vector3.zero;
            bikeRb.angularVelocity = Vector3.zero;
        }
    }

    void TeleportTo(Vector3 pos, Quaternion rot)
    {
        // 只允许 Yaw
        rot = Quaternion.Euler(0f, rot.eulerAngles.y, 0f);

        // 计算“pivot 到最低 collider”的高度，避免插地
        float yOffset = GetRootGroundOffset(bikeRoot);
        Vector3 finalPos = pos + Vector3.up * (yOffset + extraGroundOffset);

        // 先暂停物理
        if (bikeRb)
        {
            bikeRb.linearVelocity = Vector3.zero;
            bikeRb.angularVelocity = Vector3.zero;
            bikeRb.position = finalPos;
            bikeRb.rotation = rot;
        }

        bikeRoot.SetPositionAndRotation(finalPos, rot);

        // 再清一次，防止瞬间残留
        if (bikeRb)
        {
            bikeRb.linearVelocity = Vector3.zero;
            bikeRb.angularVelocity = Vector3.zero;
        }
    }

    bool TryFindSafeRespawn(Vector3 center, Quaternion facing, out Vector3 safePos, out Quaternion safeRot)
    {
        safePos = center;
        safeRot = facing;

        for (int i = 0; i < respawnTries; i++)
        {
            Vector2 rnd = Random.insideUnitCircle * respawnRadius;
            Vector3 probe = center + new Vector3(rnd.x, 0f, rnd.y);

            // 从上往下找地面
            Vector3 rayStart = probe + Vector3.up * groundRayHeight;
            if (!Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, groundRayHeight * 2f, groundMask, QueryTriggerInteraction.Ignore))
                continue;

            // 站在地上
            Vector3 candidate = hit.point;

            // 做“胶囊体”占位检测，避免刷在墙里
            if (IsBlocked(candidate)) continue;

            // 旋转保持原朝向（或也可以对齐地面方向，这里只保 yaw）
            safePos = candidate;
            safeRot = facing;
            return true;
        }

        return false;
    }

    bool IsBlocked(Vector3 groundPoint)
    {
        // 胶囊从地面上方开始
        Vector3 bottom = groundPoint + Vector3.up * 0.05f;
        Vector3 top = bottom + Vector3.up * Mathf.Max(0.1f, clearanceHeight);

        // 障碍物检测：用 obstacleMask（确保里面不包含 Ground）
        bool hit = Physics.CheckCapsule(bottom, top, clearanceRadius, obstacleMask, QueryTriggerInteraction.Ignore);
        return hit;
    }

    float GetRootGroundOffset(Transform root)
    {
        if (!root) return 0f;

        // 用 root 下所有 Collider 的 bounds 算 pivot 到最低点的高度
        Collider[] cols = root.GetComponentsInChildren<Collider>();
        if (cols == null || cols.Length == 0) return 0f;

        Bounds b = cols[0].bounds;
        for (int i = 1; i < cols.Length; i++)
        {
            if (cols[i]) b.Encapsulate(cols[i].bounds);
        }

        float lowest = b.min.y;
        float pivotY = root.position.y;
        return Mathf.Max(0f, pivotY - lowest);
    }
}
