using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] Transform playerTarget;

    private void Update()
    {
        if (playerTarget)
        {
            Vector3 startPosition = transform.position;
            Vector3 currentPosition = new Vector3(playerTarget.position.x, playerTarget.position.y + 3f, playerTarget.position.z);
            Vector3 endPosition = Vector3.MoveTowards(startPosition, currentPosition, 20 * Time.deltaTime);
            endPosition.z = startPosition.z;
            transform.position = endPosition;
        }
    }
}
