using UnityEngine;

public class mounttobike : MonoBehaviour
{
	public Transform seatPoint;
	public bool isMounted = false;

	void LateUpdate()
	{
		if (!isMounted || seatPoint == null) return;

		transform.position = seatPoint.position;
		transform.rotation = seatPoint.rotation;
	}
}
