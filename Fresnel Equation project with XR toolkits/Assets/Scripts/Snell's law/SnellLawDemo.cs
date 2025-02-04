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
    public LineRenderer refractLine;  // 折射光线的LineRenderer

    void Update()
    {
        Vector3 intersection = transform.position;

        // 计算入射方向（局部坐标系，YOZ平面，向下）
        float theta1 = Mathf.Deg2Rad * incidentAngle;
        Vector3 localIncidentDir = new Vector3(0, Mathf.Cos(theta1), Mathf.Sin(theta1)).normalized;

        // 将入射方向从局部坐标系转换到世界坐标系
        Vector3 worldIncidentDir = transform.TransformDirection(localIncidentDir);

        // 反射方向（法线为 Z 轴方向，世界坐标系）
        Vector3 normal = transform.TransformDirection(Vector3.forward); // 法线方向为 Z 轴
        Vector3 reflectDir = Vector3.Reflect(worldIncidentDir, normal);

        // 折射方向（基于折射角公式）
        Vector3 refractDir = CalculateRefractDirectionUsingTheta(theta1, normal, n1, n2, worldIncidentDir);

        // 绘制光线
        DrawLine(incidentLine, intersection, worldIncidentDir, Color.red);    // 入射
        DrawLine(reflectLine, intersection, reflectDir, Color.green);         // 反射
        if (refractDir != Vector3.zero)
            DrawLine(refractLine, intersection, refractDir, Color.blue);      // 折射
        else
            Debug.Log("Total internal Reflection happening");
    }

    Vector3 CalculateRefractDirectionUsingTheta(float theta1, Vector3 normal, float n1, float n2, Vector3 worldIncidentDir)
    {
        float eta = n1 / n2;
        float sinTheta1 = Mathf.Sin(theta1);
        float sinTheta2 = eta * sinTheta1;

        // 检查全反射
        if (Mathf.Abs(sinTheta2) > 1.0f)
        {
            return Vector3.zero;
        }

        // 计算折射角 theta2
        float theta2 = Mathf.Asin(sinTheta2);

        // 确定折射方向的符号（假设法线为 Z 轴正方向）
        // 折射方向在 YOZ 平面内，且与法线反向
        float sign = -Mathf.Sign(Vector3.Dot(worldIncidentDir, normal));

        // 构造折射方向向量
        Vector3 refractDir = new Vector3(
            0,
            -Mathf.Cos(theta2), // Z 分量（向下）
            Mathf.Sin(theta2) * sign  // Y 分量
                    
        ).normalized;

        // 转换到世界坐标系
        return transform.TransformDirection(refractDir);
    }

    void DrawLine(LineRenderer line, Vector3 start, Vector3 direction, Color color)
    {
        if (line == null) return;

        line.positionCount = 2;
        line.SetPosition(0, start);
        line.SetPosition(1, start + direction * 5);
        line.startColor = color;
        line.endColor = color;
    }
}