using UnityEngine;

public class LightFieldController : MonoBehaviour
{
    public Material lightFieldMaterial;
    [Header("动态参数")]
    public float wavelength = 632.8f;  // 使用float类型
    public float aperture = 0.5f;
    public float speed = 1.0f;

    void Update()
    {
        // 解决CS0266错误：显式转换为float
        float waveOffset = Mathf.PingPong(Time.time * speed, 200f); // 添加f后缀
        float currentWave = wavelength + waveOffset;

        // 参数传递（确保使用float）
        lightFieldMaterial.SetFloat("_WaveLength", currentWave);
        lightFieldMaterial.SetFloat("_Aperture", aperture);

        // 动态旋转效果（示例）
        transform.Rotate(Vector3.up, 10f * Time.deltaTime); // 添加f后缀
    }
}