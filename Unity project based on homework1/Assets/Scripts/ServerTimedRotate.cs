using Unity.Netcode;
using UnityEngine;

public class ServerTimedRotate : NetworkBehaviour
{
    public float degreesPerSecondX = 0;
    public float degreesPerSecondY = 20;
    public float degreesPerSecondZ = 0;
    
    // Update is called once per frame
    void Update()
    {
        if (!IsServer)
            return;
        // Your code for Exercise 1.4 here 
        float rotationX = degreesPerSecondX * Time.deltaTime;
        float rotationY = degreesPerSecondY * Time.deltaTime;
        float rotationZ = degreesPerSecondZ * Time.deltaTime;

        transform.Rotate(rotationX, rotationY, rotationZ, Space.Self);
    }
}
