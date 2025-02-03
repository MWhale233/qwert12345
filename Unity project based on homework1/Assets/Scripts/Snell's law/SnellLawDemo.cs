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

        // 折射方向（根据Snell定律手动计算，使用负向法线）
        // 折射方向（根据Snell定律手动计算，直接颠倒 Y 方向）
        Vector3 refractDir = CalculateRefractDirection(
            new Vector3(worldIncidentDir.x, worldIncidentDir.y, worldIncidentDir.z), // 直接颠倒 Y 方向
            normal, n1, n2
        );

        // 顺时针旋转折射方向 90 度
        // refractDir = Quaternion.Euler(-90, 0, 0) * refractDir;

        // 绘制光线
        DrawLine(incidentLine, intersection, worldIncidentDir, Color.red);    // 入射
        DrawLine(reflectLine, intersection, reflectDir, Color.green);         // 反射
        if (refractDir != Vector3.zero)
            DrawLine(refractLine, intersection, refractDir, Color.blue);      // 折射
        else
            Debug.Log("Total Internal Reflection happening"); // 如果折射方向为0，说明发生全反射
    }

    Vector3 CalculateRefractDirection(Vector3 incidentDir, Vector3 normal, float n1, float n2)
    {
        float eta = n1 / n2; // 折射率比
        float cosTheta1 = -Vector3.Dot(incidentDir, normal); // 入射角余弦
        float sinTheta1 = Mathf.Sqrt(1 - cosTheta1 * cosTheta1); // 入射角正弦
        float sinTheta2 = eta * sinTheta1; // 折射角正弦

        // 检查是否发生全反射
        if (sinTheta2 > 1.0f)
        {
            return Vector3.zero; // 全反射，返回零向量
        }

        float cosTheta2 = Mathf.Sqrt(1 - sinTheta2 * sinTheta2); // 折射角余弦
        Vector3 refractDir = eta * incidentDir + (eta * cosTheta1 - cosTheta2) * normal;
        return refractDir.normalized;
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