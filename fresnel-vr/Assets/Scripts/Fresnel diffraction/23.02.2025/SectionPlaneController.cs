using UnityEngine;

public class SectionPlaneController : MonoBehaviour
{
    // 新增模式枚举
    public enum RenderingMode
    {
        Mode1_UpDirection,
        Mode2_ForwardDirection
    }

    [Header("渲染模式")]
    public RenderingMode currentMode = RenderingMode.Mode1_UpDirection;
    public bool showBoundary = true;

    [Header("参考对象")]
    public Transform volumeObject;
    public Material volumeMaterial;
    public Material planeMaterial;

    [Header("控制参数")]
    [Range(0.1f, 2.0f)] public float moveSpeed = 1.0f;
    [Range(10f, 360f)] public float rotateSpeed = 90f;

    // 保存原始分界线参数
    private float originalLineWidth;
    private Color originalLineColor;

    void Start()
    {
        // 记录初始值
        originalLineWidth = planeMaterial.GetFloat("_LineWidth");
        originalLineColor = planeMaterial.GetColor("_LineColor");
    }

    void Update()
    {
        HandleMovement();
        UpdateShaderParams();
        UpdateBoundaryVisibility();
    }

    void HandleMovement()
    {
        // 移动控制代码保持不变
        float vertical = Input.GetAxis("Vertical") * moveSpeed * Time.deltaTime;
        float horizontal = Input.GetAxis("Horizontal") * moveSpeed * Time.deltaTime;
        transform.Translate(new Vector3(horizontal, 0, vertical));

        if (Input.GetMouseButton(1))
        {
            float mouseX = Input.GetAxis("Mouse X") * rotateSpeed * Time.deltaTime;
            float mouseY = Input.GetAxis("Mouse Y") * rotateSpeed * Time.deltaTime;
            transform.Rotate(Vector3.up, mouseX, Space.World);
            transform.Rotate(Vector3.right, mouseY, Space.Self);
        }
    }

    void UpdateShaderParams()
    {
        switch(currentMode)
        {
            case RenderingMode.Mode1_UpDirection:
                showBoundary = true;
                UpdateMode1();
                break;
            case RenderingMode.Mode2_ForwardDirection:
                showBoundary = false;
                UpdateMode2();
                break;
        }
    }

    void UpdateMode1()
    {
        Plane clipPlane = new Plane(transform.up, transform.position);
        Vector4 planeEquation = new Vector4(
            clipPlane.normal.x,
            clipPlane.normal.y,
            clipPlane.normal.z,
            clipPlane.distance
        );

        volumeMaterial.SetVector("_ClipPlane", planeEquation);
        planeMaterial.SetMatrix("_VolumeWorldToLocal", volumeObject.worldToLocalMatrix);
        planeMaterial.SetVector("_PlaneEquation", planeEquation);
    }

    void UpdateMode2()
    {
        Plane clipPlane = new Plane(transform.forward, transform.position);
        Vector4 planeEquation = new Vector4(
            clipPlane.normal.x,
            clipPlane.normal.y,
            clipPlane.normal.z,
            clipPlane.distance
        );

        Matrix4x4 volumeMatrix = volumeObject.worldToLocalMatrix;
        Vector3 localNormal = volumeMatrix.MultiplyVector(transform.forward).normalized;
        Vector3 localPos = volumeMatrix.MultiplyPoint(transform.position);
        float localDistance = -Vector3.Dot(localNormal, localPos);
        
        Vector4 localPlaneEquation = new Vector4(
            localNormal.x,
            localNormal.y,
            -localNormal.z,
            localDistance
        );

        volumeMaterial.SetVector("_ClipPlane", planeEquation);
        planeMaterial.SetMatrix("_VolumeWorldToLocal", volumeObject.worldToLocalMatrix);
        planeMaterial.SetVector("_PlaneEquation", localPlaneEquation);
    }

    void UpdateBoundaryVisibility()
    {
        if(currentMode == RenderingMode.Mode2_ForwardDirection && !showBoundary)
        {
            planeMaterial.SetFloat("_LineWidth", 0);
            planeMaterial.SetColor("_LineColor", new Color(
                originalLineColor.r,
                originalLineColor.g,
                originalLineColor.b,
                0
            ));
        }
        else
        {
            planeMaterial.SetFloat("_LineWidth", originalLineWidth);
            planeMaterial.SetColor("_LineColor", originalLineColor);
        }
    }

    // 清理材质参数
    void OnDestroy()
    {
        planeMaterial.SetFloat("_LineWidth", originalLineWidth);
        planeMaterial.SetColor("_LineColor", originalLineColor);
    }
}