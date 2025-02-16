using UnityEngine;
using UnityEngine.Rendering; // 添加这行

[ExecuteInEditMode]
public class VolumeController : MonoBehaviour
{
    [Header("Compute Shader")]
    public ComputeShader waveCS;
    public Material volumeMat;

    [Header("模拟参数")]
    [Tooltip("孔径半径 (mm)")] [Range(0.1f, 5f)] public float aperture = 0.5f;
    [Tooltip("波长 (nm)")] [Range(380f, 780f)] public float wavelength = 632.8f;
    [Tooltip("最大深度 (m)")] [Range(1f, 20f)] public float maxDepth = 5f;
    [Tooltip("横向范围 (m)")] [Range(1f, 10f)] public float xyScale = 3f;

    [Header("优化参数")]
    [Tooltip("3D纹理尺寸")] [Range(32, 256)] public int textureSize = 128;
    [Tooltip("采样点数")] [Range(16, 128)] public int samples = 64;
    [Tooltip("最大步数")] [Range(32, 512)] public int maxSteps = 256;
    [Tooltip("密度系数")] [Range(0.5f, 5f)] public float density = 1.5f;

    private RenderTexture volumeTex;

    void OnEnable()
    {
        #if UNITY_EDITOR
        UnityEditor.ShaderUtil.allowAsyncCompilation = false;
        #endif

        ValidateResources();
        CreateVolumeTexture();
        UpdateMaterialParameters();
        UpdateComputeParameters();
    }

    void ValidateResources()
    {
        if (waveCS == null)
            Debug.LogError("未分配Compute Shader!");
        
        if (volumeMat == null)
            Debug.LogError("未分配材质!");
    }

    void CreateVolumeTexture()
    {
        if (volumeTex != null) 
            volumeTex.Release();

        volumeTex = new RenderTexture(textureSize, textureSize, 0, RenderTextureFormat.ARGBFloat)
        {
            dimension = TextureDimension.Tex3D,
            volumeDepth = textureSize,
            enableRandomWrite = true,
            wrapMode = TextureWrapMode.Clamp,
            filterMode = FilterMode.Trilinear,
            autoGenerateMips = false
        };

        if (!volumeTex.Create())
        {
            Debug.LogError("无法创建3D纹理!");
            return;
        }

        volumeMat.SetTexture("_VolumeTex", volumeTex);
    }

    void UpdateMaterialParameters()
    {
        if (volumeMat == null) return;

        volumeMat.SetFloat("_MaxDepth", maxDepth);
        volumeMat.SetFloat("_XYScale", xyScale);
        volumeMat.SetInt("_MaxSteps", maxSteps);
        volumeMat.SetFloat("_Density", density);
        volumeMat.SetFloat("_WaveLength", wavelength);
    }

    void UpdateComputeParameters()
    {
        if (waveCS == null || !waveCS.HasKernel("CSMain")) 
            return;

        int kernel = waveCS.FindKernel("CSMain");

        // 物理参数转换
        float apertureMeters = aperture * 0.001f;    // 毫米转米
        float wavelengthMeters = wavelength * 1e-9f; // 纳米转米

        // 核心参数设置
        waveCS.SetInts("_TexSize", textureSize, textureSize, textureSize);
        waveCS.SetFloat("_ApertureRadius", apertureMeters);
        waveCS.SetFloat("_WaveLength", wavelengthMeters);
        waveCS.SetFloat("_MaxDepth", maxDepth);
        waveCS.SetFloat("_XYScale", xyScale);
        waveCS.SetInt("_Samples", samples);

        // 纹理绑定
        waveCS.SetTexture(kernel, "Result", volumeTex);

        // 计算线程组
        int threadGroups = Mathf.CeilToInt(textureSize / 8f);
        waveCS.Dispatch(kernel, threadGroups, threadGroups, threadGroups);
    }

    void OnDisable()
    {
        if (volumeTex != null)
        {
            volumeTex.Release();
            volumeTex = null;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            UpdateMaterialParameters();
            UpdateComputeParameters();
        }
    }

    #if UNITY_EDITOR
    void OnValidate()
    {
        if (!Application.isPlaying && isActiveAndEnabled)
        {
            UpdateMaterialParameters();
            UpdateComputeParameters();
        }
    }
    #endif
}