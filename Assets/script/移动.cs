using UnityEngine;

public class 移动 : MonoBehaviour
{
	public float speed = 8f;
	public float turnSpeed = 60f;

	private Rigidbody rb;

	void Start()
	{
		rb = GetComponent<Rigidbody>();
	}

	// ⚠️ 所有 Rigidbody 移动必须写在 FixedUpdate
	void FixedUpdate()
	{
		float v = Input.GetAxis("Vertical");
		float h = Input.GetAxis("Horizontal");

		// 防止极小输入导致漂移
		if (Mathf.Abs(v) < 0.01f) v = 0f;
		if (Mathf.Abs(h) < 0.01f) h = 0f;

		// 前进 / 后退
		Vector3 move = transform.forward * v * speed * Time.fixedDeltaTime;
		rb.MovePosition(rb.position + move);

		// 转向（只转 Y）
		if (h != 0)
		{
			Quaternion turn = Quaternion.Euler(0f, h * turnSpeed * Time.fixedDeltaTime, 0f);
			rb.MoveRotation(rb.rotation * turn);
		}
	}
}


