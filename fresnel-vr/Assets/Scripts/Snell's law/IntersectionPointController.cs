using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable))]
public class IntersectionPointController : MonoBehaviour
{
    public SnellLawDemo snellLawDemo;   // 关联的SnellLawDemo脚本
    public float rotationSpeed = 30f;   // 旋转速度（度/秒）
    public float angleSpeed = 10f;      // 入射角调整速度（度/秒）

    private InputAction adjustAngleAction;
    private InputAction rotateAction;
    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grabInteractable;
    private Transform controllerTransform;  // 手柄的Transform
    private Vector3 grabOffset;             // 抓取时手柄与物体的位置偏移
    private bool isGrabbed = false;

    void Start()
    {
        grabInteractable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        grabInteractable.selectEntered.AddListener(OnGrabStart);
        grabInteractable.selectExited.AddListener(OnGrabEnd);

        // 获取Input Action
        PlayerInput playerInput = GetComponent<PlayerInput>();
        InputActionAsset inputActions = playerInput.actions;

        adjustAngleAction = inputActions.FindAction("RightStickActions/AdjustIncidentAngle");
        rotateAction = inputActions.FindAction("RightStickActions/RotateIntersection");
    }

    void Update()
    {
        if (isGrabbed)
        {
            // // 手动更新物体位置，使其跟随手柄移动
            // if (controllerTransform != null)
            // {
            //     transform.position = controllerTransform.position + grabOffset;
            // }

            // 处理右摇杆输入
            float verticalInput = adjustAngleAction.ReadValue<Vector2>().y;
            AdjustIncidentAngle(verticalInput);

            float horizontalInput = rotateAction.ReadValue<Vector2>().x;
            RotateIntersection(horizontalInput);
        }
    }

    private void OnGrabStart(SelectEnterEventArgs args)
    {
        isGrabbed = true;
        adjustAngleAction.Enable();
        rotateAction.Enable();

        // 禁用 XR 默认的连续转向
        var locomotion = FindObjectOfType<ContinuousTurnProviderBase>();
        if (locomotion != null)
            locomotion.enabled = false;

        // // 记录手柄的Transform
        // controllerTransform = args.interactorObject.transform;

        // // 计算抓取时的位置偏移（手柄位置与物体位置的差）
        // grabOffset = transform.position - controllerTransform.position;

        // 禁用XRGrabInteractable的自动位置跟踪
        // grabInteractable.trackPosition = false;
    }

    private void OnGrabEnd(SelectExitEventArgs args)
    {
        isGrabbed = false;
        adjustAngleAction.Disable();
        rotateAction.Disable();

        // 恢复 XR 默认转向
        var locomotion = FindObjectOfType<ContinuousTurnProviderBase>();
        if (locomotion != null)
            locomotion.enabled = true;

        // 恢复XRGrabInteractable的默认行为（可选）
        grabInteractable.trackPosition = true;
        controllerTransform = null;
    }

    private void AdjustIncidentAngle(float input)
    {
        if (Mathf.Abs(input) > 0.1f)
        {
            snellLawDemo.incidentAngle += input * angleSpeed * Time.deltaTime;
            snellLawDemo.incidentAngle = Mathf.Clamp(snellLawDemo.incidentAngle, 0, 90);
        }
    }

    private void RotateIntersection(float input)
    {
        if (Mathf.Abs(input) > 0.1f)
        {
            float rotationAmount = input * rotationSpeed * Time.deltaTime;
            transform.Rotate(0, rotationAmount, 0);
        }
    }
}