using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class IncidentLineGrabController : MonoBehaviour
{
    [Header("引用设置")]
    // Intersection Point 对象（例如：挂有 SnellLawDemo 的对象）
    public Transform intersectionPoint;
    // SnellLawDemo 脚本实例，用于更新 incidentAngle
    public SnellLawDemo snellLawDemo;

    [Header("参数设置")]
    // 水平旋转速率（旋转角度变化/单位位移）
    public float horizontalRotationSpeed = 10f;
    // incidentAngle 变化速率（角度变化/单位垂直位移）
    public float incidentAngleChangeSpeed = 5f;
    // incidentAngle 的范围限制（单位：度）
    public float minIncidentAngle = 0.1f;
    public float maxIncidentAngle = 89f;

    // XRGrabInteractable 组件引用
    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grabInteractable;

    // 记录抓取开始时手柄的位置（世界坐标）
    private Vector3 initialInteractorPos;
    
    private bool isGrabbed = false;

    void Awake()
    {
        grabInteractable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        if (grabInteractable == null)
        {
            Debug.LogError("Incident Line 上缺少 XRGrabInteractable 组件！");
        }
        // 注册抓取事件
        grabInteractable.selectEntered.AddListener(OnSelectEntered);
        grabInteractable.selectExited.AddListener(OnSelectExited);
    }

    void OnDestroy()
    {
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.RemoveListener(OnSelectEntered);
            grabInteractable.selectExited.RemoveListener(OnSelectExited);
        }
    }

    private void OnSelectEntered(SelectEnterEventArgs args)
    {
        isGrabbed = true;
        // 记录抓取时手柄的位置
        initialInteractorPos = args.interactorObject.transform.position;
    }

    private void OnSelectExited(SelectExitEventArgs args)
    {
        isGrabbed = false;
    }

    void Update()
    {
        if (isGrabbed && grabInteractable.interactorsSelecting.Count > 0)
        {
            // 获取当前操作者的 Transform
            var interactor = grabInteractable.interactorsSelecting[0];
            Vector3 currentPos = interactor.transform.position;
            // 计算手柄位移
            Vector3 delta = currentPos - initialInteractorPos;

            // -----------------------------
            // 水平旋转：以 Intersection Point 的 forward 方向为参考
            // 正向（delta 在 forward 方向正值）时顺时针旋转；反向时逆时针旋转
            float horizontalDelta = Vector3.Dot(delta, intersectionPoint.forward);
            // 旋转 Intersection Point（绕 Y 轴旋转）
            intersectionPoint.Rotate(0f, horizontalDelta * horizontalRotationSpeed, 0f);

            // -----------------------------
            // 垂直方向：用手柄的 Y 轴位移更新 incidentAngle
            // 注意：手往下移动（delta.y 为负）时，应增大 incidentAngle；往上则减小
            float verticalDelta = delta.y;
            snellLawDemo.incidentAngle += (-verticalDelta) * incidentAngleChangeSpeed;
            snellLawDemo.incidentAngle = Mathf.Clamp(snellLawDemo.incidentAngle, minIncidentAngle, maxIncidentAngle);

            // 为了连续平滑地更新，每帧更新初始位置为当前位置
            initialInteractorPos = currentPos;
        }
    }
}
