using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BikeController : MonoBehaviour
{
	public float moveSpeed = 8f;
	public float turnSpeed = 80f;

	Rigidbody rb;

	void Awake()
	{
		rb = GetComponent<Rigidbody>();
	}

	void FixedUpdate()
	{
		float v = Input.GetAxis("Vertical");
		float h = Input.GetAxis("Horizontal");

		// 前进
		if (Mathf.Abs(v) > 0.01f)
		{
			Vector3 move = transform.forward * v * moveSpeed * Time.fixedDeltaTime;
			rb.MovePosition(rb.position + move);
		}

		// 转向（必须前进）
		if (Mathf.Abs(v) > 0.05f && Mathf.Abs(h) > 0.01f)
		{
			float turn = h * turnSpeed * Time.fixedDeltaTime;
			rb.MoveRotation(rb.rotation * Quaternion.Euler(0f, turn, 0f));
		}
	}
}
