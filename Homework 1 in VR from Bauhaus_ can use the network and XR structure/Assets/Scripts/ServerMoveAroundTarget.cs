using Unity.Netcode;
using UnityEngine;

public class ServerMoveAroundTarget : NetworkBehaviour
{
    public Transform target;

    public float degreesPerSecond = 20;

    // Update is called once per frame
    void Update()
    {
        if (!IsServer)
            return;
        var newPosition = CalculatePositionUpdate();
        var newRotation = CalculateRotationUpdate(newPosition);
        transform.position = newPosition;
        transform.rotation = newRotation;
    }

    Vector3 CalculatePositionUpdate()
    {
        // Your code for Exercise 1.2 here
        // Calculate the angle of rotation based on the time and speed
        float angle = degreesPerSecond * Time.deltaTime;
        angle = angle * Mathf.Deg2Rad;  // Convert to radians

        // Get the direction vector from the target to the current position, and ignore y axis
        Vector3 direction = transform.position - target.position;
        direction.y = 0;

        // Calculate the new position using 2D rotation formula
        float newX = direction.x * Mathf.Cos(angle) - direction.z * Mathf.Sin(angle);
        float newZ = direction.x * Mathf.Sin(angle) + direction.z * Mathf.Cos(angle);

        // Add the target position to get the final new position
        Vector3 newPosition = target.position + new Vector3(newX, 0, newZ);

        // Ensure the y-position remains unaffected
        newPosition.y = transform.position.y;

        return newPosition;
    }

    Quaternion CalculateRotationUpdate(Vector3 newPosition)
    {
        // Your code for Exercise 1.2 here
        // Calculate the direction of movement
        Vector3 movementDirection = (newPosition - transform.position).normalized;

        // If there is movement
        if (movementDirection != Vector3.zero)
        {
            // Calculate the rotation needed for the forward vector to match the movement direction
            Quaternion newRotation = Quaternion.LookRotation(movementDirection, Vector3.up);
            return newRotation;
        }

        // If there is no movement, retain the current rotation
        return transform.rotation;
    }
}
