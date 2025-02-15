using UnityEngine;

[ExecuteInEditMode]
public class Diffraction3DController : MonoBehaviour {
    [Header("渲染设置")]
    [SerializeField] private Material diffractionMaterial; // 改为私有字段+SerializeField
    public Vector3 volumeSize = new Vector3(2, 2, 10);
    [Range(16, 256)] public int resolution = 64; //64

    private Mesh volumeMesh;
    private MaterialPropertyBlock materialProperties;
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;

    void OnEnable() {
        // 确保必要组件存在
        EnsureComponents();
        
        // 初始化材质属性块
        materialProperties = new MaterialPropertyBlock();
        
        CreateVolumeMesh();
        UpdateMaterialProperties(true);
    }

    void EnsureComponents() {
        // 自动获取或添加必要组件
        if (!TryGetComponent(out meshFilter)) {
            meshFilter = gameObject.AddComponent<MeshFilter>();
        }
        if (!TryGetComponent(out meshRenderer)) {
            meshRenderer = gameObject.AddComponent<MeshRenderer>();
        }
        
        // 自动创建默认材质（如果未分配）
        if (diffractionMaterial == null) {
            diffractionMaterial = new Material(Shader.Find("Custom/3DFresnel"));
            meshRenderer.material = diffractionMaterial;
            #if UNITY_EDITOR
            Debug.LogWarning("自动创建默认材质，请保存到项目资产中");
            #endif
        }
    }

    void CreateVolumeMesh() {
        if (resolution < 16) resolution = 16;
        
        volumeMesh = new Mesh();
        volumeMesh.name = "DiffractionVolumeMesh";
        
        Vector3[] vertices = new Vector3[resolution * resolution * resolution];
        int[] indices = new int[resolution * resolution * resolution];

        for(int z=0; z<resolution; z++) {
            for(int y=0; y<resolution; y++) {
                for(int x=0; x<resolution; x++) {
                    int idx = x + y*resolution + z*resolution*resolution;
                    vertices[idx] = new Vector3(
                        (x/(float)(resolution-1)-0.5f) * volumeSize.x,
                        (y/(float)(resolution-1)-0.5f) * volumeSize.y,
                        (z/(float)(resolution-1)) * volumeSize.z
                    );
                    indices[idx] = idx;
                }
            }
        }

        volumeMesh.vertices = vertices;
        volumeMesh.SetIndices(indices, MeshTopology.Points, 0);
        meshFilter.mesh = volumeMesh;
    }

    void UpdateMaterialProperties(bool forceUpdate = false) {
        if (!isActiveAndEnabled || diffractionMaterial == null) return;
        
        // 使用属性块避免材质实例化
        meshRenderer.GetPropertyBlock(materialProperties);
        materialProperties.SetFloat("_MaxDistance", volumeSize.z);
        materialProperties.SetVector("_VolumeSize", volumeSize);
        meshRenderer.SetPropertyBlock(materialProperties);

        // 同步材质参数（如需）
        if (forceUpdate) {
            diffractionMaterial.SetFloat("_MaxDistance", volumeSize.z);
            diffractionMaterial.SetVector("_VolumeSize", volumeSize);
        }
    }

    void Update() {
        #if UNITY_EDITOR
        if (!Application.isPlaying) {
            UpdateMaterialProperties();
        }
        #endif
        
        if(transform.hasChanged) {
            UpdateMaterialProperties();
            transform.hasChanged = false;
        }
    }

    void OnValidate() {
        // 当Inspector参数变化时自动更新
        if (meshRenderer != null) {
            UpdateMaterialProperties(true);
        }
    }

    void OnDisable() {
        // 清理资源
        if (volumeMesh != null) {
            if (Application.isPlaying) {
                Destroy(volumeMesh);
            } else {
                DestroyImmediate(volumeMesh);
            }
        }
    }
}