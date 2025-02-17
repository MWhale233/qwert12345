using UnityEngine;

public class DiffractionVolumeGenerator : MonoBehaviour
{
    [Header("Source 2D Data (x,z)")]
    public Texture2D diffraction2D; 
    // 这张纹理假设已经把 I(x,z) 存在 R 通道中（或 RGBA 中），
    // uv.x in [0,1] -> x in [-a,a]
    // uv.y in [0,1] -> z in [0,zMax]

    [Header("Volume Settings")]
    public int resolutionR = 128;   // 径向分辨率
    public int resolutionPhi = 128; // 方位角分辨率
    public int resolutionZ = 128;   // z方向分辨率

    public float apertureRadius = 0.5f; // 与 shader 保持一致
    public float zMax = 1.0f;          // 与 shader 保持一致

    private Texture3D volumeTex;

    void Start()
    {
        volumeTex = new Texture3D(resolutionR, resolutionPhi, resolutionZ, 
                                  TextureFormat.RFloat, false);
        volumeTex.wrapMode = TextureWrapMode.Clamp;

        // 准备一个数组来接收所有体素颜色 (R 通道即可)
        Color[] volumeColors = new Color[resolutionR * resolutionPhi * resolutionZ];

        // 三重循环，遍历 (rIndex, phiIndex, zIndex)
        int idx = 0;
        for (int iz = 0; iz < resolutionZ; iz++)
        {
            // z in [0, zMax]
            float zFrac = (float)iz / (resolutionZ - 1);
            float zVal = zFrac * zMax;

            for (int iphi = 0; iphi < resolutionPhi; iphi++)
            {
                // phi in [0, 2π)
                float phiFrac = (float)iphi / (resolutionPhi);
                // float phiVal = phiFrac * 2.0f * Mathf.PI; // 如需用到phi本身

                for (int ir = 0; ir < resolutionR; ir++)
                {
                    // r in [0, apertureRadius]
                    float rFrac = (float)ir / (resolutionR - 1);
                    float rVal = rFrac * apertureRadius;

                    // 从 2D 纹理 (x,z) 采样：
                    // x = ±rVal，都一样，因为衍射2D图在 x>=0 / x<=0 应该是对称
                    // 所以我们只取 x>=0 那半边数据 (或先前做好的对称)
                    float xNorm = (rVal + apertureRadius) / (2.0f * apertureRadius);
                    // zNorm = zVal / zMax
                    float zNorm = zVal / zMax;

                    // 在 diffraction2D 中采样
                    // 假设 diffraction2D 的 uv=(0,0) 对应 x=-a, z=0
                    // uv=(1,1) 对应 x=+a, z=zMax
                    // 则 xNorm = 0.5 + rVal/(2*a)
                    // 这里简化写法：apertureRadius = a
                    // => xNorm = 0.5 + (rVal/(2*a))
                    float uvX = 0.5f + 0.5f * (rVal / apertureRadius);
                    float uvY = zNorm;

                    // 取得强度
                    Color c = diffraction2D.GetPixelBilinear(uvX, uvY);
                    float intensity = c.r; // 假设红通道存强度

                    // 存入体素
                    volumeColors[idx] = new Color(intensity, 0, 0, intensity);
                    idx++;
                }
            }
        }

        // 将数组写入 3D 纹理
        volumeTex.SetPixels(volumeColors);
        volumeTex.Apply();

        // 把 volumeTex 赋给一个材质属性 (给体渲染Shader使用)
        GetComponent<Renderer>().material.SetTexture("_VolumeTex", volumeTex);
    }
}
