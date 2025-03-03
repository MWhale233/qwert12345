// using UnityEngine;
// using UnityEngine.Rendering; // 添加这行

// public class WavePropagationController : MonoBehaviour {
//     public ComputeShader waveCS;
//     public int textureWidth = 64;
//     public int textureHeight = 64;
//     public int textureDepth = 64;

//     [Header("Parameters")]
//     [Range(0.1f, 1f)] public float apertureRadius = 0.3f;
//     [Range(300, 700)] public float waveLength = 632.8f;
//     public float maxDepth = 10f;
//     [Range(16, 128)] public int samples = 64;

//     private RenderTexture volumeTexture;

//     void Start() {
//         // 创建3D纹理
//         volumeTexture = new RenderTexture(textureWidth, textureHeight, 0, RenderTextureFormat.ARGBFloat);
//         volumeTexture.dimension = TextureDimension.Tex3D;
//         volumeTexture.volumeDepth = textureDepth;
//         volumeTexture.enableRandomWrite = true;
//         volumeTexture.wrapMode = TextureWrapMode.Clamp;
//         volumeTexture.filterMode = FilterMode.Point;
//         volumeTexture.Create();

//         if (!volumeTexture.IsCreated()) {
//             Debug.LogError("Texture创建失败！");
//             return;
//         }

//         // 初始化纹理为绿色
//         RenderTexture.active = volumeTexture;
//         GL.Clear(true, true, Color.green);
//         RenderTexture.active = null;

//         // 设置Compute Shader参数
//         int kernel = waveCS.FindKernel("CSMain");
//         waveCS.SetInts("_TexSize", textureWidth, textureHeight, textureDepth);
//         waveCS.SetFloat("_ApertureRadius", apertureRadius);
//         waveCS.SetFloat("_WaveLength", waveLength * 1e-9f);
//         waveCS.SetFloat("_MaxDepth", maxDepth);
//         waveCS.SetInt("_Samples", samples);
//         waveCS.SetTexture(kernel, "Result", volumeTexture);

//         // 计算Dispatch次数
//         uint threadX, threadY, threadZ;
//         waveCS.GetKernelThreadGroupSizes(kernel, out threadX, out threadY, out threadZ);
//         int dispatchX = Mathf.CeilToInt(textureWidth / (float)threadX);
//         int dispatchY = Mathf.CeilToInt(textureHeight / (float)threadY);
//         int dispatchZ = Mathf.CeilToInt(textureDepth / (float)threadZ);

//         waveCS.Dispatch(kernel, dispatchX, dispatchY, dispatchZ);
//     }

//     void OnDestroy() {
//         if (volumeTexture != null) {
//             volumeTexture.Release();
//         }
//     }
// }



using UnityEngine;

public class WavePropagationController : MonoBehaviour
{
    public ComputeShader waveCS;
    // 输出体数据的分辨率
    public int textureWidth = 128;
    public int textureHeight = 128;
    public int textureDepth = 64;

    [Header("光学参数")]
    [Range(0.1f, 1f)]
    public float apertureRadius = 0.5f;
    [Range(300, 700)]
    public float waveLength = 500f; // 单位：nm
    public float maxDepth = 10f;
    [Tooltip("孔径积分采样点数")]
    public int samples = 32;

    // 生成的3D纹理
    public RenderTexture volumeTexture;

    void Start() {
        volumeTexture = new RenderTexture(textureWidth, textureHeight, 0, RenderTextureFormat.ARGB32);
        volumeTexture.dimension = UnityEngine.Rendering.TextureDimension.Tex3D;
        volumeTexture.volumeDepth = textureDepth;
        volumeTexture.enableRandomWrite = true;
        volumeTexture.wrapMode = TextureWrapMode.Clamp;
        volumeTexture.filterMode = FilterMode.Point;
        volumeTexture.Create();

        if (!volumeTexture.IsCreated()) {
            Debug.LogError("VolumeTexture 创建失败！");
            return;
        }

        // 绑定 Compute Shader 并执行初始化
        int kernel = waveCS.FindKernel("CSMain"); //InitTexture
        waveCS.SetInts("_TexSize", textureWidth, textureHeight, textureDepth);
        waveCS.SetTexture(kernel, "Result", volumeTexture);

        int threadX = Mathf.CeilToInt(textureWidth / 8.0f);
        int threadY = Mathf.CeilToInt(textureHeight / 8.0f);
        int threadZ = Mathf.CeilToInt(textureDepth / 8.0f);

        waveCS.Dispatch(kernel, threadX, threadY, threadZ);

        Debug.Log("3D 纹理已初始化为绿色");
    }
}
    // void Start() {
    //     volumeTexture = new RenderTexture(textureWidth, textureHeight, 0, RenderTextureFormat.ARGB32);
    //     volumeTexture.dimension = UnityEngine.Rendering.TextureDimension.Tex3D;
    //     volumeTexture.volumeDepth = textureDepth;
    //     volumeTexture.enableRandomWrite = true;
    //     volumeTexture.wrapMode = TextureWrapMode.Clamp;
    //     volumeTexture.filterMode = FilterMode.Point; // 避免插值
    //     volumeTexture.Create();

    //     // 强制初始化纹理为绿色
    //     RenderTexture.active = volumeTexture;
    //     GL.Clear(true, true, Color.green);
    //     RenderTexture.active = null;
    // }

    // void Start()
    // {
    //     // 创建 3D RenderTexture（注意格式选择 Float 类型以存储高精度数据）
    //     // volumeTexture = new RenderTexture(textureWidth, textureHeight, 0, RenderTextureFormat.RFloat);
    //     volumeTexture = new RenderTexture(textureWidth, textureHeight, 0, RenderTextureFormat.ARGBFloat);

    //     volumeTexture.dimension = UnityEngine.Rendering.TextureDimension.Tex3D;
    //     volumeTexture.volumeDepth = textureDepth;
    //     volumeTexture.enableRandomWrite = true;
    //     volumeTexture.wrapMode = TextureWrapMode.Clamp;
    //     volumeTexture.filterMode = FilterMode.Bilinear;
    //     volumeTexture.Create();

    //     if (!volumeTexture.IsCreated())
    //     {
    //         Debug.LogError("VolumeTexture 创建失败！");
    //     }
    //     else
    //     {
    //         Debug.Log("VolumeTexture 创建成功！");
    //     }

    //     // 设置 Compute Shader 参数
    //     int kernel = waveCS.FindKernel("CSMain");
    //     waveCS.SetInts("_TexSize", textureWidth, textureHeight, textureDepth);
    //     waveCS.SetFloat("_ApertureRadius", apertureRadius);
    //     waveCS.SetFloat("_WaveLength", waveLength);
    //     waveCS.SetFloat("_MaxDepth", maxDepth);
    //     waveCS.SetInt("_Samples", samples);
    //     waveCS.SetTexture(kernel, "Result", volumeTexture);

    //     // 根据 3D 纹理尺寸计算 Dispatch 数量（这里 numthreads 设为 8,8,8）
    //     int threadX = Mathf.CeilToInt(textureWidth / 8.0f);
    //     int threadY = Mathf.CeilToInt(textureHeight / 8.0f);
    //     int threadZ = Mathf.CeilToInt(textureDepth / 8.0f);
    //     Debug.Log($"Dispatch: {threadX}, {threadY}, {threadZ}");
    //     waveCS.Dispatch(kernel, threadX, threadY, threadZ);

    //     // 将生成的 volumeTexture 挂载到你体渲染材质上（例如通过 Shader 的 _VolumeTex 参数）
    //     // 例如：GetComponent<Renderer>().material.SetTexture("_VolumeTex", volumeTexture);
    // }

    // 如果想在运行时调整参数并重新计算，可以在 Update 或响应 UI 后重新 Dispatch
    // void Update()
    // {
    //     // 示例：允许实时调整时，每帧更新参数（实际使用中建议优化，仅在参数变化时更新）
    //     int kernel = waveCS.FindKernel("CSMain");
    //     waveCS.SetFloat("_ApertureRadius", apertureRadius);
    //     waveCS.SetFloat("_WaveLength", waveLength);
    //     waveCS.SetFloat("_MaxDepth", maxDepth);
    //     waveCS.SetInt("_Samples", samples);
    //     waveCS.SetTexture(kernel, "Result", volumeTexture);

    //     int threadX = Mathf.CeilToInt(textureWidth / 8.0f);
    //     int threadY = Mathf.CeilToInt(textureHeight / 8.0f);
    //     int threadZ = Mathf.CeilToInt(textureDepth / 8.0f);
    //     waveCS.Dispatch(kernel, threadX, threadY, threadZ);
    // }
// }
