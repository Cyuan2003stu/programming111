using UnityEngine;

public class player : MonoBehaviour
{
	void LateUpdate()
	{
		transform.localRotation = Quaternion.identity;
	}
}

