using UnityEngine;

public class Camera_Follow : MonoBehaviour
{
    public float FollowSpeed = 2f;
    public Transform Target;

    // Update is called once per frame
    void Update()
    {
        Vector3 targetPosition = new Vector3(Target.position.x, Target.position.y, transform.position.z);
        transform.position = Vector3.Lerp(transform.position, targetPosition, FollowSpeed * Time.deltaTime);
    }
}
