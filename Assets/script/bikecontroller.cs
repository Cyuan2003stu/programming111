using UnityEngine;
public class bikecontroller : MonoBehaviour
{
	public float speed = 8f;
	public float turnSpeed = 60f;

	public Animator playerAnimator;
	public float moveThreshold = 0.1f;

	void FixedUpdate()
	{
		float v = Input.GetAxis("Vertical");
		float h = Input.GetAxis("Horizontal");

		transform.Translate(Vector3.forward * v * speed * Time.fixedDeltaTime);
		transform.Rotate(Vector3.up * h * turnSpeed * Time.fixedDeltaTime);

		bool LandSoft = Mathf.Abs(v) > moveThreshold;

		if (playerAnimator != null)
		{
			playerAnimator.SetBool("IsRiding", LandSoft);
		}
	}
}
