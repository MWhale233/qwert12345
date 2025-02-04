using Unity.Netcode;
using UnityEngine;

public class ServerGazeSimulation : NetworkBehaviour
{
    public float gazeChangeFrequency = 2f;

    private float lastGazeChange;

    private Quaternion gazeOriginRotation;

    private Quaternion gazeTargetRotation;

    // Start is called before the first frame update
    void Start()
    {
        ChangeGazeDirection();
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsServer)
            return;
        
        float timeSinceChange = Time.time - lastGazeChange;
        if (timeSinceChange > gazeChangeFrequency)
        {
            ChangeGazeDirection();
        }
        else
        {
            float lerpFactor = timeSinceChange / gazeChangeFrequency;
            transform.localRotation = Quaternion.Lerp(gazeOriginRotation, gazeTargetRotation, lerpFactor);
        }
    }

    private void ChangeGazeDirection()
    {
        gazeOriginRotation = transform.localRotation;
        gazeTargetRotation = Quaternion.LookRotation(GetRandomGazeDirection(), Vector3.up);
        lastGazeChange = Time.time;
    }

    private Vector3 GetRandomGazeDirection()
    {
        var x = Random.Range(-1.5f, 1.5f);
        var y = Random.Range(-1.5f, 1.5f);
        return new Vector3(x, y, 1.0f).normalized;
    }
}
