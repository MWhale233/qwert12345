// VolumeDiffractionController.cs
using UnityEngine;

[ExecuteInEditMode]
public class VolumeDiffractionController : MonoBehaviour {
    public Material volumeMat;
    public Transform observer;
    public float transitionSpeed = 1f;

    void Update() {
        // 自动调整观察参数
        float zDepth = Mathf.Clamp(observer.position.z, 0.1f, 10f);
        volumeMat.SetFloat("_MaxDepth", zDepth);
        
        // 动态孔径效果（可选）
        float pulse = Mathf.PingPong(Time.time, 1);
        volumeMat.SetFloat("_ApertureRadius", 0.3f + pulse * 0.1f);
    }

    // 处理用户输入
    void HandleInput() {
        float move = Input.GetAxis("Vertical") * transitionSpeed * Time.deltaTime;
        observer.Translate(0, 0, move);
    }
}