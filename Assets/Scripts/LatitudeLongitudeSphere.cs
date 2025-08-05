// LatitudeLongitudeSphere.cs
// AutoWireframeSphere
using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting; // 添加List支持

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class AutoWireframeSphere : MonoBehaviour
{
    [Header("网格设置")]
    [Range(4, 64)] public int longitudeSegments = 16;
    [Range(3, 32)] public int latitudeSegments = 8;
    [Range(0.001f, 0.1f)] public float lineWidth = 0.02f;

    [Header("特殊经线设置")]
    public Color primeMeridianColor = Color.red; // 0度经线颜色
    [Range(0, 1)] public float colorBlendWidth = 0.1f; // 颜色过渡宽度

    [Header("高级设置")]
    public bool updateInRuntime = true;

    // 公共字段
    public string celestialName;
    public Material wireframeMaterial;
    public CelestialData.BodyData celestialBodyData;
    public float abs_radius;

    // 私有变量
    private Mesh mesh;
    private Renderer sphereRenderer;
    private Material specialLineMaterial; // 特殊经线材质

    private bool on_mouse = false;
    private bool on_celestial = false;
    private bool on_construction = false;

    void Start()
    {
        InitializeComponents();
        GenerateWireframe();
        // 确保物体在正确的层级
        gameObject.layer = LayerMask.NameToLayer("Celestials");
    }


    // 鼠标交互
    private void OnMouseEnter()
    {
        on_mouse = true;
        // Debug.Log("Enter"+on_mouse.ToString());
    }

    private void OnMouseExit()
    {
        on_mouse = false;
        // Debug.Log("Exit"+on_mouse.ToString());
    }

    public void GenerateWireframe()
    {
        InitializeComponents();

        if (celestialBodyData == null)
        {
            Debug.LogWarning("celestialBodyData is null for: " + celestialName);
            return;
        }

        // 使用修正半径
        float actualRadius = CoordinateManager.Instance.GetBodyRadius(celestialBodyData);
        // Debug.Log(celestialBodyData.ToString());
        // CoordinateManager.Instance.GetBodyRadius(celestialBodyData);

        // 获取球体颜色
        Color sphereColor = GetSphereColor();
        wireframeMaterial.SetColor("_LineColor", sphereColor);
        wireframeMaterial.SetFloat("_LineWidth", lineWidth);

        // 生成网格数据
        // Debug.Log(actualRadius);
        GenerateMeshData(actualRadius);
    }


    public void InitializeComponents()
    {
        // 获取或创建必要的组件
        if (mesh == null)
        {
            mesh = new Mesh();
            mesh.name = "WireframeSphere";
        }

        sphereRenderer = GetComponent<Renderer>();

        // 确保有MeshFilter组件
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        if (meshFilter == null) meshFilter = gameObject.AddComponent<MeshFilter>();
        meshFilter.mesh = mesh;

        // 确保有MeshRenderer组件
        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer == null) meshRenderer = gameObject.AddComponent<MeshRenderer>();

        // 创建或获取线框材质
        if (wireframeMaterial == null)
        {
            wireframeMaterial = new Material(Shader.Find("Custom/LatLongWireframe"));
        }

        // 创建特殊经线材质
        if (specialLineMaterial == null)
        {
            specialLineMaterial = new Material(wireframeMaterial);
            specialLineMaterial.SetColor("_LineColor", primeMeridianColor);
        }

        // 设置材质数组
        meshRenderer.sharedMaterials = new Material[] { wireframeMaterial, specialLineMaterial };
    }

    void Update()
    {
        gameObject.GetComponent<Rigidbody>().velocity = Vector3.zero;
        // Debug.Log("UpDate");
        // 更新特殊经线颜色
        if (specialLineMaterial != null)
        {
            specialLineMaterial.SetColor("_LineColor", primeMeridianColor);
        }

        // 如果启用运行时更新，检查球体参数是否变化
        if (updateInRuntime && HasSphereParametersChanged())
        {
            GenerateWireframe();
        }

        // Debug.Log(celestialName+on_mouse.ToString());

        if (on_mouse)
        {
            // 使用射线检测确定实际命中的物体
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                //Debug.Log("Hit"+celestialName+hit.collider.gameObject.layer.ToString()+"/"+LayerMask.NameToLayer("Celestial").ToString());
                // 检查是否命中了当前天体（父物体）
                if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Celestials"))
                {
                    on_construction = false;
                    on_celestial = true;
                    UIManager.Instance.ShowCelestialInfo(this);
                    Debug.Log("ShowCelestialInfo:" + celestialName);
                }

                // 检查是否命中了建筑（子物体）
                else if (celestialName == CoordinateManager.Instance.focusbodyname)
                {
                    if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Constructions"))
                    {
                        on_construction = true;
                        on_celestial = false;
                        Debug.Log("HideCelestialInfo:" + celestialName);
                        // 获取建筑组件
                        OrbitalConstruction construction = hit.collider.GetComponent<OrbitalConstruction>();
                        construction.ShowConstructionInfo();
                    }
                }
            }
        }

        else if (celestialName == CoordinateManager.Instance.focusbodyname)
        {
            UIManager.Instance.HideCelestialInfo();
            UIManager.Instance.HideConstructionInfo();

        }

        // 疑似冲突
        
        if (celestialBodyData != null)
        {
            transform.position = celestialBodyData.display_pos;
            Debug.Log(celestialName + ":" + celestialBodyData.display_pos.ToString());
            transform.localScale = Vector3.one * celestialBodyData.display_radius;
        }
        

        if (celestialName == CoordinateManager.Instance.focusbodyname)
        {
            foreach (Transform child in transform) // 直接遍历transform的子级
            {
                OrbitalConstruction orbitalConstruction = child.gameObject.GetComponent<OrbitalConstruction>();

                // 计算建筑在球面上的位置
                Vector3 position = Create.Instance.CalculateSurfacePosition(
                    Vector3.zero,
                    abs_radius / 600000,
                    orbitalConstruction.latitude,
                    orbitalConstruction.longitude
                );
                child.gameObject.transform.position = position;
            }
        }
    }

    bool HasSphereParametersChanged()
    {
        return transform.hasChanged;
    }


    Color GetSphereColor()
    {
        if (wireframeMaterial != null && wireframeMaterial.HasProperty("_LineColor"))
        {
            return wireframeMaterial.GetColor("_LineColor");
        }

        if (sphereRenderer != null && sphereRenderer.sharedMaterial != null &&
            sphereRenderer.sharedMaterial.HasProperty("_LineColor"))
        {
            return sphereRenderer.sharedMaterial.GetColor("_LineColor");
        }

        return Color.white;
    }

    void GenerateMeshData(float radius)
    {

        // 顶点计算
        int vertexCount = longitudeSegments * (latitudeSegments + 1)  // 经线
                        + (latitudeSegments - 1) * longitudeSegments; // 纬线 (排除两极)

        Vector3[] vertices = new Vector3[vertexCount];
        int vertexIndex = 0;

        // 生成经线顶点
        for (int lon = 0; lon < longitudeSegments; lon++)
        {
            float phi = 2f * Mathf.PI * lon / longitudeSegments;

            for (int lat = 0; lat <= latitudeSegments; lat++)
            {
                float theta = Mathf.PI * lat / latitudeSegments;
                vertices[vertexIndex++] = SphericalToCartesian(radius, theta, phi);
            }
        }

        // 生成纬线顶点 (排除两极)
        for (int lat = 1; lat < latitudeSegments; lat++)
        {
            float theta = Mathf.PI * lat / latitudeSegments;

            for (int lon = 0; lon < longitudeSegments; lon++)
            {
                float phi = 2f * Mathf.PI * lon / longitudeSegments;
                vertices[vertexIndex++] = SphericalToCartesian(radius, theta, phi);
            }
        }

        // 创建子网格索引
        List<int> mainIndices = new List<int>(); // 主网格索引（普通颜色）
        List<int> specialIndices = new List<int>(); // 特殊经线索引（红色）

        // 经线索引
        for (int lon = 0; lon < longitudeSegments; lon++)
        {
            int startIndex = lon * (latitudeSegments + 1);

            for (int lat = 0; lat < latitudeSegments; lat++)
            {
                int idx1 = startIndex + lat;
                int idx2 = startIndex + lat + 1;

                // 检查是否为0度经线（lon == 0）
                if (lon == 0)
                {
                    specialIndices.Add(idx1);
                    specialIndices.Add(idx2);
                }
                else
                {
                    mainIndices.Add(idx1);
                    mainIndices.Add(idx2);
                }
            }
        }

        // 纬线索引 (全部普通颜色)
        int baseIndex = longitudeSegments * (latitudeSegments + 1);
        for (int lat = 0; lat < latitudeSegments - 1; lat++)
        {
            int latStart = baseIndex + lat * longitudeSegments;

            for (int lon = 0; lon < longitudeSegments; lon++)
            {
                int current = latStart + lon;
                int next = latStart + (lon + 1) % longitudeSegments;

                mainIndices.Add(current);
                mainIndices.Add(next);
            }
        }

        // 更新网格
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.subMeshCount = 2; // 两个子网格

        // 设置主网格（普通线）
        mesh.SetIndices(mainIndices.ToArray(), MeshTopology.Lines, 0);

        // 设置特殊网格（0度经线）
        mesh.SetIndices(specialIndices.ToArray(), MeshTopology.Lines, 1);

        SphereCollider collider = GetComponent<SphereCollider>();
        collider.radius = radius;
    }

    Vector3 SphericalToCartesian(float radius, float theta, float phi)
    {
        float x = radius * Mathf.Sin(theta) * Mathf.Cos(phi);
        float y = radius * Mathf.Cos(theta);
        float z = radius * Mathf.Sin(theta) * Mathf.Sin(phi);
        return new Vector3(x, y, z);

    }
    
    public void UpdateWireframe()
    {
        if (celestialBodyData == null) return;
        
        // 获取当前显示半径
        float actualRadius = CoordinateManager.Instance.GetBodyRadius(celestialBodyData);
        
        // 更新线框
        GenerateMeshData(actualRadius);
        
        // 更新碰撞器
        SphereCollider collider = GetComponent<SphereCollider>();
        if (collider != null)
        {
            collider.radius = actualRadius;
        }
    }
}