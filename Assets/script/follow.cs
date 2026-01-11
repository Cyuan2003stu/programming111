using UnityEngine;

public class FollowSeatPoint : MonoBehaviour
{
    public Transform seatPoint;
    public bool followRotation = true;

    void LateUpdate()
    {
        if (!seatPoint) return;

        transform.position = seatPoint.position;

        if (followRotation)
            transform.rotation = seatPoint.rotation;
    }
}
