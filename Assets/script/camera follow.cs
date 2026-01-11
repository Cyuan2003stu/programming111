using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;   // bike
    public Vector3 offset = new Vector3(0, 2.5f, -5f);
    public float posSmooth = 5f;
    public float rotSmooth = 6f;

    void LateUpdate()
    {
        // ===== 1. 计算“水平 forward”（关键）=====
        Vector3 flatForward = target.forward;
        flatForward.y = 0f;          // ❗ 去掉倾斜导致的上下分量
        flatForward.Normalize();

        // ===== 2. 用“水平 forward”算位置（不会下降）=====
        Vector3 desiredPos =
            target.position +
            flatForward * offset.z +
            Vector3.up * offset.y;

        transform.position = Vector3.Lerp(
            transform.position,
            desiredPos,
            Time.deltaTime * posSmooth
        );

        // ===== 3. 相机朝向（同样只跟 Y 轴）=====
        Quaternion targetRot = Quaternion.LookRotation(flatForward, Vector3.up);

        transform.rotation = Quaternion.Lerp(
            transform.rotation,
            targetRot,
            Time.deltaTime * rotSmooth
        );
    }
}
