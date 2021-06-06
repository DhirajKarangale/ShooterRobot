using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] float followSpeed = 20;
    private Transform playerTarget;

    private void Awake()
    {
        playerTarget = GameObject.Find("Robot").transform;
    }

    private void Update()
    {
        if (playerTarget)
        {
            Vector3 startPosition = transform.position;
            Vector3 currentPosition = new Vector3(playerTarget.position.x, playerTarget.position.y + 3f, playerTarget.position.z);
            Vector3 endPosition = Vector3.MoveTowards(startPosition, currentPosition, followSpeed * Time.deltaTime);
            endPosition.z = startPosition.z;
            transform.position = endPosition;
        }
    }
}
