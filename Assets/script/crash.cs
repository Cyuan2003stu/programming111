using UnityEngine;
using System.Collections;

public class BikeCrashHandler : MonoBehaviour
{
    [Header("������")]
    public float crashSpeed = 2f;          // ��������ٶȲ���ˤ��
    public float recoveryOffset = 2f;      // ����λ�õ����ƫ��

    [Header("���")]
    public Rigidbody rb;
    public Animator playerAnimator;
    public GameObject player;
    public MonoBehaviour[] movementScripts;    // ����ƶ��ű���
    public CanvasGroup blackScreen;           // UI ����

    private bool crashed = false;

    void Start()
    {
        if (rb == null) rb = GetComponent<Rigidbody>();
    }

    void OnCollisionEnter(Collision collision)
    {
        if (crashed) return;

        // �ٶ��㹻��Ŵ���ˤ��
        if (rb.linearVelocity.magnitude < crashSpeed) return;

        // �κ����嶼����
        StartCoroutine(CrashProcess());
    }

    IEnumerator CrashProcess()
    {
        crashed = true;

        // ֹͣ�ƶ��ű�
        foreach (var s in movementScripts)
            s.enabled = false;

        // ����ˤ������
        if (playerAnimator != null)
            playerAnimator.SetTrigger("Fall");

        // ��һ�����ó�����
        rb.AddForce(transform.right * 3f + Vector3.up * 2f, ForceMode.Impulse);
        rb.AddTorque(transform.forward * 200f);

        // ���� 0.5 �뽥��
        yield return StartCoroutine(FadeBlackScreen(1, 0.5f));

        // �� 1.5 �루����״̬��
        yield return new WaitForSeconds(1.5f);

        // ��� + �� �ͻ�λ�ã������һЩ��
        Vector3 revivePos = transform.position - transform.forward * recoveryOffset;
        revivePos.y += 1f;

        transform.position = revivePos;
        transform.rotation = Quaternion.identity;

        player.transform.localPosition = Vector3.zero;

        // ͣ��
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        // ����վ������
        playerAnimator.SetTrigger("Idle");

        // ��������
        yield return StartCoroutine(FadeBlackScreen(0, 0.5f));

        // �ָ��ƶ��ű�
        foreach (var s in movementScripts)
            s.enabled = true;

        crashed = false;
    }

    IEnumerator FadeBlackScreen(float target, float duration)
    {
        float start = blackScreen.alpha;
        float t = 0;
        while (t < duration)
        {
            t += Time.deltaTime;
            blackScreen.alpha = Mathf.Lerp(start, target, t / duration);
            yield return null;
        }
    }
}
