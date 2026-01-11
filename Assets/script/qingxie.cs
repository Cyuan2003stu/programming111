using UnityEngine;

public class qingxie : MonoBehaviour
{

      public float maxLean = 15f;   // 最大倾斜角度
      public float leanSpeed = 5f;

      float currentLean = 0f;
 void Update()
        {
       float h = Input.GetAxis("Horizontal");

       float targetLean = -h * maxLean; // 左转右倾
            currentLean = Mathf.Lerp(currentLean, targetLean, Time.deltaTime * leanSpeed);

            transform.localRotation = Quaternion.Euler(0f, 0f, currentLean);
        }
    }