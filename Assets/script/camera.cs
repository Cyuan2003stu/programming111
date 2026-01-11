using UnityEngine;

public class camera : MonoBehaviour
{
	public Transform player;
	public float height = 50f;

	void LateUpdate()
	{
		transform.position = new Vector3(
			player.position.x,
			height,
			player.position.z
		);
	}
}