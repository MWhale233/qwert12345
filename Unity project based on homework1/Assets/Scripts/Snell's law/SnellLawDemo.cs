using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class SnellLawDemo : MonoBehaviour
{
    public float n1 = 1.0f; // 上层介质折射率
    public float n2 = 1.5f; // 下层介质折射率
    public float incidentAngle = 30f; // 入射角度（度数）

    public LineRenderer incidentLine; // 入射光线的LineRenderer
    public LineRenderer reflectLine;  // 反射光线的LineRenderer

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame

    void Update()
    {
        Vector3 intersection = transform.position;

        // 计算入射方向（YOZ平面，向下）
        float theta1 = Mathf.Deg2Rad * incidentAngle;
        Vector3 incidentDir = new Vector3(0, Mathf.Cos(theta1), Mathf.Sin(theta1)).normalized;

        // 反射方向（法线为 Z 轴方向）
        Vector3 normal = Vector3.forward; // 法线方向为 Z 轴
        Vector3 reflectDir = Vector3.Reflect(incidentDir, normal);

        // 绘制光线
        DrawLine(incidentLine, intersection, incidentDir, Color.red);    // 入射
        DrawLine(reflectLine, intersection, reflectDir, Color.green);   // 反射
    }

    void DrawLine(LineRenderer line, Vector3 start, Vector3 direction, Color color)
    {
        if (line == null) return;

        line.positionCount = 2; // 设置LineRenderer的点数
        line.SetPosition(0, start); // 起点
        line.SetPosition(1, start + direction * 5); // 终点（光线长度为5）
        line.startColor = color;
        line.endColor = color;
    }
}