using UnityEngine;

public class rider : MonoBehaviour
{
	public Animator riderAnimator;
	public float moveSpeed = 5f;

	void Update()
	{
		float input = Input.GetAxis("Vertical");

		bool isMoving = Mathf.Abs(input) > 0.01f;

		// 控制自行车移动
		transform.Translate(Vector3.forward * input * moveSpeed * Time.deltaTime);

		// 控制骑车动画
		riderAnimator.SetBool("IsRiding", isMoving);
	}
}
