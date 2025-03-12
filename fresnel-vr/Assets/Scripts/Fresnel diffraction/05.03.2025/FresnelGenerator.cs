using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.IntegralTransforms;
using System.Runtime.InteropServices;

using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;

#if UNITY_EDITOR
using UnityEditor;
using System.IO;
#endif

[RequireComponent(typeof(Renderer))]
public class FresnelGenerator : MonoBehaviour
{
    [Header("VR Input")]
    public InputActionProperty radiusAction;
    public InputActionProperty renderAction;

    [Header("Simulation Parameters")]
    public float wavelength = 0.632e-6f;
    public float radius = 0.0009f;
    public int N = 256;
    public float L = 0.0036f;
    public int numZ = 100;
    public float zMin = 0.05f;
    public float zMax = 0.6f;

    [Header("Visualization")]
    [Range(0.1f, 5.0f)]
    public float intensityScale = 1.0f;

    [Header("Materials")]
    public Material volumeMaterial;
    public Material planeMaterial;

#if UNITY_EDITOR
    [Header("Editor Settings")]
    public string textureSavePath = "Assets/VolumeTextures/NewGeneratedTexture3D.asset";
    public bool autoSaveTexture = true;
#endif

    private Texture3D volumeTexture;
    private Complex32[,] initialAperture;
    private float dx;
    private bool isCalculating = false;

    // 暴露计算状态的公共属性
    public bool IsCalculating => isCalculating;

    void Start()
    {
        MathNet.Numerics.Control.UseNativeMKL();
        InitializeAperture();
    }



    void InitializeAperture()
    {
        dx = L / N;
        initialAperture = new Complex32[N, N];
        
        float halfL = L / 2;
        float radiusSq = radius * radius;
        
        for (int i = 0; i < N; i++)
        {
            float x = -halfL + i * dx;
            for (int j = 0; j < N; j++)
            {
                float y = -halfL + j * dx;
                initialAperture[i, j] = (x * x + y * y) <= radiusSq 
                    ? new Complex32(1, 0) 
                    : Complex32.Zero;
            }
        }
    }

    // 公共方法供按钮调用
    public void GenerateVolume()
    {
        if (!isCalculating)
        {
            InitializeAperture();
            StartCoroutine(CalculateDiffractionVolume());
        }
        
    }

    // 修改后的方法
    public void ChangeAperture(float newRadius)
    {
        // 应用Slider的值并限制范围
        radius = Mathf.Clamp(newRadius * 0.0001f, 0.0001f, 0.0009f);
        
        // 更新孔径形状
        InitializeAperture();
        
        // 如果需要实时更新衍射效果（根据性能需求决定是否启用）
        // if (!isCalculating) StartCoroutine(CalculateDiffractionVolume());
    }
        
        
    IEnumerator CalculateDiffractionVolume()
    {
        isCalculating = true;
        
        // 销毁旧的纹理资源
        if (volumeTexture != null)
        {
    #if UNITY_EDITOR
            DestroyImmediate(volumeTexture, true); // 允许销毁资源
    #else
            Destroy(volumeTexture); // 在运行时只能销毁实例化的对象
    #endif
            volumeTexture = null;
        }

        volumeTexture = new Texture3D(N, N, numZ, TextureFormat.RFloat, false);
        volumeTexture.wrapMode = TextureWrapMode.Clamp;
        
        float[] volumeData = new float[N * N * numZ];
        float maxIntensity = 0;

        for (int zIdx = 0; zIdx < numZ; zIdx++)
        {
            float z = Mathf.Lerp(zMin, zMax, (float)zIdx / (numZ - 1));
            Complex32[,] field = FresnelPropagate(initialAperture, z, wavelength, dx);
            
            // 计算并记录强度，寻找最大值用于归一化
            for (int i = 0; i < N; i++)
            {
                for (int j = 0; j < N; j++)
                {
                    float intensity = field[i, j].MagnitudeSquared;
                    maxIntensity = Mathf.Max(maxIntensity, intensity);
                    volumeData[zIdx * N * N + i * N + j] = intensity;
                }
            }

            if (zIdx % 10 == 0) // 每10层暂停一下，防止卡顿
                yield return null;
        }

        // 数据归一化
        for (int i = 0; i < volumeData.Length; i++)
            volumeData[i] /= maxIntensity;

        volumeTexture.SetPixelData(volumeData, 0);
        volumeTexture.Apply();

        ApplyMaterials();
        
    #if UNITY_EDITOR
        if (autoSaveTexture)
        {
            // 延迟一帧确保资源创建完成
            EditorApplication.delayCall += () =>
            {
                SaveTextureAsset();
                EditorApplication.delayCall = null;
            };
        }
    #endif

        isCalculating = false;
        yield return null;
    }

    void ApplyMaterials()
    {
        // 将纹理赋值给当前 Renderer 材质
        if (TryGetComponent<Renderer>(out var renderer))
        {
            renderer.material.SetTexture("_VolumeTex", volumeTexture);
            renderer.material.SetFloat("_IntensityScale", intensityScale);
        }

        // 应用材质参数
        if (volumeMaterial != null)
        {
            volumeMaterial.SetTexture("_VolumeTex", volumeTexture);
            volumeMaterial.SetFloat("_Alpha", 0.05f);
            volumeMaterial.SetFloat("_StepSize", 0.01f);
        }

        if (planeMaterial != null)
        {
            planeMaterial.SetTexture("_VolumeTex", volumeTexture);
            planeMaterial.SetFloat("_Alpha", 0.05f);
            planeMaterial.SetFloat("_StepSize", 0.01f);
        }
    }

#if UNITY_EDITOR
void SaveTextureAsset()
{
    if (volumeTexture == null)
    {
        Debug.LogError("无法保存空纹理！");
        return;
    }

    string directory = Path.GetDirectoryName(textureSavePath);
    if (!Directory.Exists(directory))
    {
        Directory.CreateDirectory(directory);
    }

    Texture3D existing = AssetDatabase.LoadAssetAtPath<Texture3D>(textureSavePath);
    if (existing != null)
    {
        // 更新已有资源
        EditorUtility.CopySerialized(volumeTexture, existing);
        Debug.Log("更新已有纹理资源：" + textureSavePath);
    }
    else
    {
        // 创建新资源
        AssetDatabase.CreateAsset(volumeTexture, textureSavePath);
        Debug.Log("创建新纹理资源：" + textureSavePath);
    }
    
    AssetDatabase.SaveAssets();
    AssetDatabase.Refresh();

    // 更新材质并通知 SectionPlaneController
    ApplyMaterials();
    UpdateSectionPlaneController();

    
}

void UpdateSectionPlaneController()
{
    // 查找场景中的 SectionPlaneController
    SectionPlaneController controller = FindObjectOfType<SectionPlaneController>();
    if (controller != null)
    {
        // 更新材质引用
        controller.volumeMaterial = volumeMaterial;
        controller.planeMaterial = planeMaterial;

        // 重新计算裁剪平面
        controller.UpdateShaderParams();
        Debug.Log("SectionPlaneController 已更新材质并重新计算裁剪平面！");
    }
    else
    {
        Debug.LogWarning("未找到 SectionPlaneController！");
    }
}
#endif

    Complex32[,] FresnelPropagate(Complex32[,] u0, float z, float lambda, float dx)
    {
        int size = u0.GetLength(0);
        Complex32[,] result = new Complex32[size, size];
        Complex32[] temp = new Complex32[size * size];

        // 手动拷贝二维数组到一维数组
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                temp[i * size + j] = u0[i, j];
            }
        }

        // FFT
        Fourier.Forward2D(temp, size, size);

        // 构造传递函数并应用
        float k = 2 * Mathf.PI / lambda;
        float scaleFactor = -Mathf.PI * lambda * z;
        
        for (int i = 0; i < size; i++)
        {
            float fy = (i - size / 2) / (size * dx);
            for (int j = 0; j < size; j++)
            {
                float fx = (j - size / 2) / (size * dx);
                float phase = scaleFactor * (fx * fx + fy * fy);
                Complex32 h = new Complex32(Mathf.Cos(k * z + phase), Mathf.Sin(k * z + phase));
                temp[i * size + j] *= h;
            }
        }

        // IFFT
        Fourier.Inverse2D(temp, size, size);

        // 手动拷贝一维数组回二维数组
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                result[i, j] = temp[i * size + j];
            }
        }

        return result;
    }
}