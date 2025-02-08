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

    // 新增属性，供其他脚本调用
    public Vector3 IncidentDirection { get; private set; }
    public Vector3 ReflectDirection { get; private set; }
    public Vector3 RefractDirection { get; private set; }
    public float ReflectAngle { get; private set; }   // 与法线的夹角（度数）
    public float RefractAngle { get; private set; }     // 与法线的夹角（度数）

    void Update()
    {
        Vector3 intersection = transform.position;

        // 计算入射方向（局部坐标系，YOZ平面，向下）
        float theta1 = Mathf.Deg2Rad * incidentAngle;
        Vector3 localIncidentDir = new Vector3(0, Mathf.Cos(theta1), Mathf.Sin(theta1)).normalized;
        // 转换到世界坐标系
        IncidentDirection = transform.TransformDirection(localIncidentDir);

        // 法线方向（假设为Z轴正方向）
        Vector3 normal = transform.TransformDirection(Vector3.forward);

        // 反射方向
        ReflectDirection = Vector3.Reflect(IncidentDirection, normal);
        // 计算反射角（与法线的夹角）
        ReflectAngle = Vector3.Angle(ReflectDirection, normal);

        // 折射方向（计算方法见下）
        RefractDirection = CalculateRefractDirectionUsingTheta(theta1, normal, n1, n2, IncidentDirection);
        if (RefractDirection != Vector3.zero)
        {
            // 注意：这里计算折射角时，可以根据需要调整参考法线方向（例如取 -normal）
            RefractAngle = Vector3.Angle(RefractDirection, -normal);
        }
        else
        {
            RefractAngle = 0f;
        }

        // 绘制光线
        DrawLine(incidentLine, intersection, IncidentDirection, Color.red);    // 入射光线
        DrawLine(reflectLine, intersection, ReflectDirection, Color.green);      // 反射光线
        if (RefractDirection != Vector3.zero)
            DrawLine(refractLine, intersection, RefractDirection, Color.blue);   // 折射光线
        else
            Debug.Log("发生全反射");

        Debug.Log($"Incident Dir: {IncidentDirection}");
        Debug.Log($"Reflect Dir: {ReflectDirection}");
        Debug.Log($"Refract Dir: {RefractDirection}");
        Debug.Log($"Reflect Angle: {ReflectAngle - 90}");
        Debug.Log($"Refract Angle: {90 - RefractAngle}");

    }

    Vector3 CalculateRefractDirectionUsingTheta(float theta1, Vector3 normal, float n1, float n2, Vector3 incidentDir)
    {
        float eta = n1 / n2;
        float sinTheta1 = Mathf.Sin(theta1);
        float sinTheta2 = eta * sinTheta1;

        // 检查全反射情况
        if (Mathf.Abs(sinTheta2) > 1.0f)
        {
            return Vector3.zero;
        }

        // 计算折射角 theta2
        float theta2 = Mathf.Asin(sinTheta2);

        // 根据入射方向与法线的点积确定折射方向的符号
        float sign = -Mathf.Sign(Vector3.Dot(incidentDir, normal));

        // 构造局部折射方向（仍在YOZ平面内）
        Vector3 localRefractDir = new Vector3(
            0,
            -Mathf.Cos(theta2), // 对应 Y 分量
            Mathf.Sin(theta2) * sign  // 对应 Z 分量
        ).normalized;

        // 转换到世界坐标系返回
        return transform.TransformDirection(localRefractDir);
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
