using UnityEngine;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class VolumeDataLoader : MonoBehaviour
{
    // 数据文件路径（建议放在 Resources 或 StreamingAssets 文件夹中）
    public string filePath = "Assets/Field Data/fresnel_diffraction_intensity_normalized.raw";
    public string textureSavePath = "Assets/VolumeTextures/GeneratedTexture3D.asset";
    public int width = 256;  // x 方向分辨率
    public int height = 256; // y 方向分辨率
    public int depth = 100;  // z 方向采样点数

    // 用于显示体数据的材质，该材质应使用体渲染 shader
    public Material volumeMaterial;
    public Material planeMaterial;

    void Start()
    {
        // 读取文件
        byte[] fileData = File.ReadAllBytes(filePath);
        int numElements = width * height * depth;
        // 检查数据大小是否匹配：32位浮点数，每个 float 4 字节
        if(fileData.Length != numElements * 4)
        {
            Debug.LogError("数据文件大小与预期尺寸不匹配！");
            return;
        }

        // 将字节数组转换为 float 数组
        float[] floatData = new float[numElements];
        System.Buffer.BlockCopy(fileData, 0, floatData, 0, fileData.Length);

        // 将数据映射到 3D 纹理的颜色数组中
        // 这里假设数据已经归一化到适合显示的范围，如果没有，可能需要归一化处理
        Color[] colors = new Color[numElements];
        for (int i = 0; i < numElements; i++)
        {
            // 用灰度表示强度，alpha 固定为 1
            float intensity = floatData[i];
            colors[i] = new Color(intensity, intensity, intensity, 1.0f);
        }

        // 创建 3D 纹理（建议使用 TextureFormat.RFloat 或者 TextureFormat.RGBAFloat，根据数据需求）
        Texture3D volumeTexture = new Texture3D(width, height, depth, TextureFormat.RFloat, false)
        {
            wrapMode = TextureWrapMode.Clamp // 关键设置：禁止纹理重复
        };
        volumeTexture.SetPixels(colors);
        volumeTexture.Apply();
        #if UNITY_EDITOR
                // 确保目录存在
                Directory.CreateDirectory(Path.GetDirectoryName(textureSavePath));
                // 删除旧资源（如果存在）
                if(File.Exists(textureSavePath)) 
                    AssetDatabase.DeleteAsset(textureSavePath);
                // 创建新资源
                AssetDatabase.CreateAsset(volumeTexture, textureSavePath);
                AssetDatabase.SaveAssets();
                Debug.Log("3D 纹理已保存至：" + textureSavePath);
        #endif

        // 将生成的 3D 纹理赋值给材质
        if (volumeMaterial != null)
        {
            volumeMaterial.SetTexture("_VolumeTex", volumeTexture);

            // 动态设置其他Shader参数（可选）
            volumeMaterial.SetFloat("_Alpha", 0.05f);
            volumeMaterial.SetFloat("_StepSize", 0.01f);

            Debug.Log("3D纹理已应用到指定 volumeMaterial！");
        }
        else
        {
            Debug.LogWarning("没有指定 volumeMaterial！");
        }

        
        // 将生成的 3D 纹理赋值给材质
        if (planeMaterial != null)
        {
            planeMaterial.SetTexture("_VolumeTex", volumeTexture);

            // 动态设置其他Shader参数（可选）
            planeMaterial.SetFloat("_Alpha", 0.05f);
            planeMaterial.SetFloat("_StepSize", 0.01f);

            Debug.Log("3D纹理已应用到指定 planeMaterial");
        }
        else
        {
            Debug.LogWarning("没有指定 planeMaterial");
        }
    }
}