using UnityEngine;
using System.Collections;

public class WhiteScreenCollision : MonoBehaviour
{
    public CanvasGroup whiteScreen;
    public float fadeSpeed = 6f;
    public float holdTime = 1.5f;

    public LayerMask ignoreLayers; // 这里存放地面、人行道、道路标记

    bool flashing = false;

    private void OnCollisionEnter(Collision collision)
    {
        GameObject hit = collision.collider.gameObject;

        // Debug 输出你撞到的是什么
        Debug.Log("<color=yellow>撞到了：</color> " + hit.name + " [Layer=" + LayerMask.LayerToName(hit.layer) + "]");

        // 如果是在忽略层内，则不触发
        if ((ignoreLayers.value & (1 << hit.layer)) != 0)
        {
            return;
        }

        if (!flashing)
            StartCoroutine(FlashWhite());
    }

    IEnumerator FlashWhite()
    {
        flashing = true;

        // 渐入白
        while (whiteScreen.alpha < 1f)
        {
            whiteScreen.alpha += Time.deltaTime * fadeSpeed;
            yield return null;
        }

        yield return new WaitForSeconds(holdTime);

        // 渐出白
        while (whiteScreen.alpha > 0f)
        {
            whiteScreen.alpha -= Time.deltaTime * fadeSpeed;
            yield return null;
        }

        flashing = false;
    }
}
