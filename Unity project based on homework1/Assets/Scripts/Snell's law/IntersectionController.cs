using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IntersectionController  : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // 固定Y坐标为0
        Vector3 pos = transform.position;
        pos.y = 0;
        transform.position = pos;
    }
}
