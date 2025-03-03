using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IntersectionController : MonoBehaviour
{
    void Update()
    {
        // 固定 Y 坐标为 0
        Vector3 pos = transform.position;
        pos.y = 0;
        transform.position = pos;

        // 限制旋转：只允许绕 Y 轴旋转
        Vector3 eulerAngles = transform.eulerAngles;
        eulerAngles.x = 0;   // 锁定 X 轴旋转
        eulerAngles.z = 0;   // 锁定 Z 轴旋转
        transform.eulerAngles = eulerAngles;
    }
}