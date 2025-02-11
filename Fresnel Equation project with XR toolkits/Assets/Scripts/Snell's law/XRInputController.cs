using UnityEngine;
using UnityEngine.InputSystem;

public class XRInputController : MonoBehaviour
{
    public SnellLawDemo snellLawDemo;       // 关联的SnellLawDemo脚本
    public Transform intersectionPoint;    // 交点物体
    public float rotationSpeed = 30f;      // 旋转速度（度/秒）
    public float angleSpeed = 10f;         // 入射角调整速度（度/秒）

    private InputAction rotateAction;
    private InputAction angleAction;

    void Start()
    {
        // 获取输入 Action
        InputActionAsset inputActions = GetComponent<PlayerInput>().actions;
        rotateAction = inputActions.FindAction("RotateIntersection");
        angleAction = inputActions.FindAction("AdjustIncidentAngle");
    }

    void Update()
    {
        // 处理摇杆左右输入（旋转交点）
        float rotateInput = rotateAction.ReadValue<Vector2>().x;
        RotateIntersection(rotateInput);

        // 处理摇杆上下输入（调整入射角）
        float angleInput = angleAction.ReadValue<Vector2>().y;
        AdjustIncidentAngle(angleInput);
    }

    private void RotateIntersection(float input)
    {
        if (Mathf.Abs(input) > 0.1f) // 摇杆死区过滤
        {
            float rotationAmount = input * rotationSpeed * Time.deltaTime;
            intersectionPoint.Rotate(0, rotationAmount, 0);
        }
    }

    private void AdjustIncidentAngle(float input)
    {
        if (Mathf.Abs(input) > 0.1f) // 摇杆死区过滤
        {
            snellLawDemo.incidentAngle += input * angleSpeed * Time.deltaTime;
            snellLawDemo.incidentAngle = Mathf.Clamp(snellLawDemo.incidentAngle, 0, 90);
        }
    }
}