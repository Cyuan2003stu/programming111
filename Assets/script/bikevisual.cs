using UnityEngine;

public class BikeLean : MonoBehaviour
{
	public float maxLeanAngle = 10f;
	public float leanSmooth = 6f;

	float currentLean;

	void Update()
	{
		float h = Input.GetAxis("Horizontal");

		float targetLean = -h * maxLeanAngle;

		currentLean = Mathf.Lerp(
			currentLean,
			targetLean,
			Time.deltaTime * leanSmooth
		);

		Vector3 rot = transform.localEulerAngles;
		rot.z = currentLean;
		transform.localEulerAngles = rot;
	}
}