using UnityEngine;

public class 按键 : MonoBehaviour
{
	[Header("Lean Settings")]
	public float maxLeanAngle = 20f;   // 最大倾斜角（度）
	public float leanSpeed = 6f;        // 倾斜/回正速度

	float currentLean;

	void Update()
	{
		bool w = Input.GetKey(KeyCode.W);
		bool a = Input.GetKey(KeyCode.A);
		bool d = Input.GetKey(KeyCode.D);

		float targetLean = 0f;

		// 只有在前进时才允许倾斜
		if (w)
		{
			if (a) targetLean = +maxLeanAngle; // 左倾
			if (d) targetLean = -maxLeanAngle; // 右倾
		}

		// 平滑过渡
		currentLean = Mathf.Lerp(
			currentLean,
			targetLean,
			Time.deltaTime * leanSpeed
		);

		// 只改 Z 轴（压弯）
		Vector3 rot = transform.localEulerAngles;
		rot.z = currentLean;
		transform.localEulerAngles = rot;
	}
}
