using UnityEngine;

using System.Collections;
using UnityEngine;

public class WhiteScreenFlash : MonoBehaviour
{
    public CanvasGroup whiteCanvas;   // 白色Image上的CanvasGroup
    public float fadeInTime = 0.05f;  // 从无到全白的时间
    public float holdTime = 2f;       // 保持白屏的时间（你要的2秒）
    public float fadeOutTime = 0.2f;  // 从白到正常的时间

    bool isRunning = false;

    void Awake()
    {
        if (whiteCanvas == null)
            whiteCanvas = GetComponent<CanvasGroup>();

        if (whiteCanvas != null)
            whiteCanvas.alpha = 0f;
    }

    // 在“摔倒”时调用这个方法
    public void TriggerFlash()
    {
        if (!isRunning && whiteCanvas != null)
        {
            StartCoroutine(FlashRoutine());
        }
    }

    IEnumerator FlashRoutine()
    {
        isRunning = true;

        // 渐变到全白
        float t = 0f;
        while (t < fadeInTime)
        {
            t += Time.deltaTime;
            whiteCanvas.alpha = Mathf.Lerp(0f, 1f, t / fadeInTime);
            yield return null;
        }
        whiteCanvas.alpha = 1f;

        // 保持 2 秒
        yield return new WaitForSeconds(holdTime);

        // 渐变回正常
        t = 0f;
        while (t < fadeOutTime)
        {
            t += Time.deltaTime;
            whiteCanvas.alpha = Mathf.Lerp(1f, 0f, t / fadeOutTime);
            yield return null;
        }
        whiteCanvas.alpha = 0f;

        isRunning = false;
    }
}
