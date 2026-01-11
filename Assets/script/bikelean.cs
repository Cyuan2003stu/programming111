using UnityEngine;

public class bikelean : MonoBehaviour
{
	[Header("Lean Settings")]
	public float maxLeanAngle = 20f;   // 最大倾斜角
	public float leanSpeed = 5f;        // 倾斜/回正速度

	[Header("Input")]
	public string horizontalAxis = "Horizontal";

	float currentLean;

	void Update()
	{
		float input = Input.GetAxis(horizontalAxis);

		// 目标倾斜角（右转为负更自然）
		float targetLean = -input * maxLeanAngle;

		// 平滑过渡
		currentLean = Mathf.Lerp(currentLean, targetLean, Time.deltaTime * leanSpeed);

		// 只改 Z 轴
		Vector3 rot = transform.localEulerAngles;
		rot.z = currentLean;
		transform.localEulerAngles = rot;
	}
}
