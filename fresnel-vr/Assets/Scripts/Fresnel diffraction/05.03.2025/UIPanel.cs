using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Canvas))]
public class UIPanel : MonoBehaviour
{
    [Header("控制器绑定")]
    public Transform rightHandController;

    [Header("UI元素")]
    public Slider radiusSlider;
    public Button generateButton;

    private Canvas uiCanvas;
    private Transform controllerTransform;

    void Start()
    {
        uiCanvas = GetComponent<Canvas>();
        uiCanvas.worldCamera = Camera.main;
        uiCanvas.renderMode = RenderMode.WorldSpace;

        // 绑定到右手控制器
        controllerTransform = rightHandController.transform;
        AttachToController();

        // 确保UI一直显示
        uiCanvas.enabled = true;
    }

    void Update()
    {
        // 保持UI朝向用户
        transform.LookAt(Camera.main.transform);
        transform.Rotate(0, 180, 0); // 翻转面板
    }

    private void AttachToController()
    {
        // 设置UI位置和大小
        transform.SetParent(controllerTransform);
        transform.localPosition = new Vector3(0, 0.1f, 0.1f); // 调整到合适位置
        transform.localRotation = Quaternion.identity;
        transform.localScale = new Vector3(1f, 1f, 1f);
    }

    public void SetupInteraction(FresnelGenerator generator)
    {
        // 滑块配置
        radiusSlider.minValue = 0.0001f;
        radiusSlider.maxValue = 0.005f;
        radiusSlider.value = generator.radius;
        
        radiusSlider.onValueChanged.AddListener(value => {
            generator.radius = value;
        });

        // 按钮配置
        generateButton.onClick.AddListener(() => {
            generator.GenerateVolume();
        });
    }
}