using UnityEngine;

public class DecalCullingManager : MonoBehaviour
{
    [Header("Culling Target")]
    public Transform target;          // Player 或 Camera

    [Header("Distance Settings")]
    [Range(1f, 100f)]
    public float activeRange = 20f;   // 可调距离（米）

    [Header("Performance")]
    public bool useSqrDistance = true;

    MonoBehaviour[] decalScripts;
    Transform[] decalTransforms;

    void Start()
    {
        GameObject[] decalObjects = GameObject.FindGameObjectsWithTag("Decal");

        decalScripts = new MonoBehaviour[decalObjects.Length];
        decalTransforms = new Transform[decalObjects.Length];

        for (int i = 0; i < decalObjects.Length; i++)
        {
            decalTransforms[i] = decalObjects[i].transform;

            // 找到这个物体上的第一个“Decal 脚本”
            var scripts = decalObjects[i].GetComponents<MonoBehaviour>();
            foreach (var s in scripts)
            {
                if (s != null && s.GetType().Name.Contains("Decal"))
                {
                    decalScripts[i] = s;
                    break;
                }
            }
        }
    }

    void Update()
    {
        if (target == null) return;

        float range = useSqrDistance ? activeRange * activeRange : activeRange;

        for (int i = 0; i < decalScripts.Length; i++)
        {
            if (decalScripts[i] == null) continue;

            float dist = useSqrDistance
                ? (decalTransforms[i].position - target.position).sqrMagnitude
                : Vector3.Distance(decalTransforms[i].position, target.position);

            bool shouldEnable = dist < range;

            if (decalScripts[i].enabled != shouldEnable)
            {
                decalScripts[i].enabled = shouldEnable;
            }
        }
    }
}
