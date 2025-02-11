using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FresnelLawDemo : MonoBehaviour
{
    // 通过 Inspector 将同一 GameObject 上的 SnellLawDemo 脚本拖入此引用，
    // 或者在 Start() 中用 GetComponent 自动获取
    public SnellLawDemo snellLawDemo;

    // Fresnel 计算结果
    private float R_s;           // s-偏振反射系数
    private float R_p;           // p-偏振反射系数
    private float R_unpolarized; // 未偏振光的反射率
    private float T;             // 透射率

    void Start()
    {
        if (snellLawDemo == null)
        {
            snellLawDemo = GetComponent<SnellLawDemo>();
        }
    }

    void Update()
    {
        if (snellLawDemo == null)
        {
            Debug.LogError("未找到 SnellLawDemo 组件！");
            return;
        }

        // 从 SnellLawDemo 中获取介质折射率和入射角（度数）
        float n1 = snellLawDemo.n1;
        float n2 = snellLawDemo.n2;
        float incidentAngleDeg = snellLawDemo.incidentAngle;

        // 获取偏振类型
        SnellLawDemo.PolarizationType polarizationType = snellLawDemo.polarization;
        

        // 入射角转换为弧度
        float theta_i = incidentAngleDeg * Mathf.Deg2Rad;

        // 根据 Snell 定律：n1*sin(theta_i) = n2*sin(theta_t)
        float sinTheta_t = (n1 / n2) * Mathf.Sin(theta_i);
        if (Mathf.Abs(sinTheta_t) > 1.0f)
        {
            // Debug.Log("TIR happening_Fresnel");
            return;
        }
        float theta_t = Mathf.Asin(sinTheta_t);

        // 计算余弦值
        float cos_theta_i = Mathf.Cos(theta_i);
        float cos_theta_t = Mathf.Cos(theta_t);

        // Fresnel 公式：
        // Rₛ = ((n1*cos(theta_i) - n2*cos(theta_t)) / (n1*cos(theta_i) + n2*cos(theta_t)))²
        // Rₚ = ((n2*cos(theta_i) - n1*cos(theta_t)) / (n2*cos(theta_i) + n1*cos(theta_t)))²
        R_s = Mathf.Pow((n1 * cos_theta_i - n2 * cos_theta_t) / (n1 * cos_theta_i + n2 * cos_theta_t), 2);
        R_p = Mathf.Pow((n2 * cos_theta_i - n1 * cos_theta_t) / (n2 * cos_theta_i + n1 * cos_theta_t), 2);

        // 根据偏振类型选择反射率
        float R = 0f;
        switch (polarizationType)
        {
            case SnellLawDemo.PolarizationType.S:
                R = R_s;
                break;
            case SnellLawDemo.PolarizationType.P:
                R = R_p;
                break;
            default:
                // Debug.LogError("未知的偏振类型！");
                return;
        }

        // 透射率 T = 1 - R
        T = 1 - R;

        UpdateLineAlpha();

        // 输出计算过程和结果
        // Debug.Log("Fresnel Equation Calculation:");
        // Debug.Log($"入射角（弧度） theta_i = {theta_i:F4}");
        // Debug.Log($"折射角（弧度） theta_t = {theta_t:F4}");
        // Debug.Log($"cos(theta_i) = {cos_theta_i:F4}");
        // Debug.Log($"cos(theta_t) = {cos_theta_t:F4}");
        // Debug.Log($"Rₛ = (({n1}*{cos_theta_i:F4} - {n2}*{cos_theta_t:F4}) / ({n1}*{cos_theta_i:F4} + {n2}*{cos_theta_t:F4}))² = {R_s:F4}");
        // Debug.Log($"Rₚ = (({n2}*{cos_theta_i:F4} - {n1}*{cos_theta_t:F4}) / ({n2}*{cos_theta_i:F4} + {n1}*{cos_theta_t:F4}))² = {R_p:F4}");
        // Debug.Log($"当前偏振类型：{polarizationType}");
        // Debug.Log($"反射率 R = {R:F4}");
        // Debug.Log($"透射率 T = 1 - R = {T:F4}");


    }

    void UpdateLineAlpha()
    {
        bool totalInternalReflection = Mathf.Abs((snellLawDemo.n1 / snellLawDemo.n2) * Mathf.Sin(snellLawDemo.incidentAngle * Mathf.Deg2Rad)) > 1.0f;

        if (snellLawDemo.reflectLine != null)
        {
            Material reflectMaterial = snellLawDemo.reflectLine.material;
            Color reflectColor = reflectMaterial.color;

            if (totalInternalReflection)
            {
                reflectColor.a = 1; // 全反射时，反射光完全可见
            }
            else
            {
                reflectColor.a = R_s; // 正常情况，透明度由 R_s 决定
            }

            reflectMaterial.color = reflectColor;
            reflectMaterial.SetFloat("_Mode", 3);
            reflectMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            reflectMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            reflectMaterial.EnableKeyword("_ALPHABLEND_ON");
            reflectMaterial.renderQueue = 3100;

            // Debug.Log($"反射光透明度 {reflectColor.a}");
        }

        if (snellLawDemo.refractLine != null)
        {
            Material refractMaterial = snellLawDemo.refractLine.material;
            Color refractColor = refractMaterial.color;

            refractColor.a = totalInternalReflection ? 0 : T; // 全反射时，折射光不可见，否则透明度 = 透射率 T

            refractMaterial.color = refractColor;
            refractMaterial.SetFloat("_Mode", 3);
            refractMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            refractMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            refractMaterial.EnableKeyword("_ALPHABLEND_ON");
            refractMaterial.renderQueue = 3100;

            // Debug.Log($"折射光透明度 {refractColor.a}");
        }
    }



}