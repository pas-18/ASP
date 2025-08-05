// LatitudeLongitudeSphere.cs
// AutoWireframeSphere
using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting; // ���List֧��

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class AutoWireframeSphere : MonoBehaviour
{
    [Header("��������")]
    [Range(4, 64)] public int longitudeSegments = 16;
    [Range(3, 32)] public int latitudeSegments = 8;
    [Range(0.001f, 0.1f)] public float lineWidth = 0.02f;

    [Header("���⾭������")]
    public Color primeMeridianColor = Color.red; // 0�Ⱦ�����ɫ
    [Range(0, 1)] public float colorBlendWidth = 0.1f; // ��ɫ���ɿ��

    [Header("�߼�����")]
    public bool updateInRuntime = true;

    // �����ֶ�
    public string celestialName;
    public Material wireframeMaterial;
    public CelestialData.BodyData celestialBodyData;
    public float abs_radius;

    // ˽�б���
    private Mesh mesh;
    private Renderer sphereRenderer;
    private Material specialLineMaterial; // ���⾭�߲���

    private bool on_mouse = false;
    private bool on_celestial = false;
    private bool on_construction = false;

    void Start()
    {
        InitializeComponents();
        GenerateWireframe();
        // ȷ����������ȷ�Ĳ㼶
        gameObject.layer = LayerMask.NameToLayer("Celestials");
    }


    // ��꽻��
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

        // ʹ�������뾶
        float actualRadius = CoordinateManager.Instance.GetBodyRadius(celestialBodyData);
        // Debug.Log(celestialBodyData.ToString());
        // CoordinateManager.Instance.GetBodyRadius(celestialBodyData);

        // ��ȡ������ɫ
        Color sphereColor = GetSphereColor();
        wireframeMaterial.SetColor("_LineColor", sphereColor);
        wireframeMaterial.SetFloat("_LineWidth", lineWidth);

        // ������������
        // Debug.Log(actualRadius);
        GenerateMeshData(actualRadius);
    }


    public void InitializeComponents()
    {
        // ��ȡ�򴴽���Ҫ�����
        if (mesh == null)
        {
            mesh = new Mesh();
            mesh.name = "WireframeSphere";
        }

        sphereRenderer = GetComponent<Renderer>();

        // ȷ����MeshFilter���
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        if (meshFilter == null) meshFilter = gameObject.AddComponent<MeshFilter>();
        meshFilter.mesh = mesh;

        // ȷ����MeshRenderer���
        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer == null) meshRenderer = gameObject.AddComponent<MeshRenderer>();

        // �������ȡ�߿����
        if (wireframeMaterial == null)
        {
            wireframeMaterial = new Material(Shader.Find("Custom/LatLongWireframe"));
        }

        // �������⾭�߲���
        if (specialLineMaterial == null)
        {
            specialLineMaterial = new Material(wireframeMaterial);
            specialLineMaterial.SetColor("_LineColor", primeMeridianColor);
        }

        // ���ò�������
        meshRenderer.sharedMaterials = new Material[] { wireframeMaterial, specialLineMaterial };
    }

    void Update()
    {
        gameObject.GetComponent<Rigidbody>().velocity = Vector3.zero;
        // Debug.Log("UpDate");
        // �������⾭����ɫ
        if (specialLineMaterial != null)
        {
            specialLineMaterial.SetColor("_LineColor", primeMeridianColor);
        }

        // �����������ʱ���£������������Ƿ�仯
        if (updateInRuntime && HasSphereParametersChanged())
        {
            GenerateWireframe();
        }

        // Debug.Log(celestialName+on_mouse.ToString());

        if (on_mouse)
        {
            // ʹ�����߼��ȷ��ʵ�����е�����
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                //Debug.Log("Hit"+celestialName+hit.collider.gameObject.layer.ToString()+"/"+LayerMask.NameToLayer("Celestial").ToString());
                // ����Ƿ������˵�ǰ���壨�����壩
                if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Celestials"))
                {
                    on_construction = false;
                    on_celestial = true;
                    UIManager.Instance.ShowCelestialInfo(this);
                    Debug.Log("ShowCelestialInfo:" + celestialName);
                }

                // ����Ƿ������˽����������壩
                else if (celestialName == CoordinateManager.Instance.focusbodyname)
                {
                    if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Constructions"))
                    {
                        on_construction = true;
                        on_celestial = false;
                        Debug.Log("HideCelestialInfo:" + celestialName);
                        // ��ȡ�������
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

        // ���Ƴ�ͻ
        
        if (celestialBodyData != null)
        {
            transform.position = celestialBodyData.display_pos;
            Debug.Log(celestialName + ":" + celestialBodyData.display_pos.ToString());
            transform.localScale = Vector3.one * celestialBodyData.display_radius;
        }
        

        if (celestialName == CoordinateManager.Instance.focusbodyname)
        {
            foreach (Transform child in transform) // ֱ�ӱ���transform���Ӽ�
            {
                OrbitalConstruction orbitalConstruction = child.gameObject.GetComponent<OrbitalConstruction>();

                // ���㽨���������ϵ�λ��
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

        // �������
        int vertexCount = longitudeSegments * (latitudeSegments + 1)  // ����
                        + (latitudeSegments - 1) * longitudeSegments; // γ�� (�ų�����)

        Vector3[] vertices = new Vector3[vertexCount];
        int vertexIndex = 0;

        // ���ɾ��߶���
        for (int lon = 0; lon < longitudeSegments; lon++)
        {
            float phi = 2f * Mathf.PI * lon / longitudeSegments;

            for (int lat = 0; lat <= latitudeSegments; lat++)
            {
                float theta = Mathf.PI * lat / latitudeSegments;
                vertices[vertexIndex++] = SphericalToCartesian(radius, theta, phi);
            }
        }

        // ����γ�߶��� (�ų�����)
        for (int lat = 1; lat < latitudeSegments; lat++)
        {
            float theta = Mathf.PI * lat / latitudeSegments;

            for (int lon = 0; lon < longitudeSegments; lon++)
            {
                float phi = 2f * Mathf.PI * lon / longitudeSegments;
                vertices[vertexIndex++] = SphericalToCartesian(radius, theta, phi);
            }
        }

        // ��������������
        List<int> mainIndices = new List<int>(); // ��������������ͨ��ɫ��
        List<int> specialIndices = new List<int>(); // ���⾭����������ɫ��

        // ��������
        for (int lon = 0; lon < longitudeSegments; lon++)
        {
            int startIndex = lon * (latitudeSegments + 1);

            for (int lat = 0; lat < latitudeSegments; lat++)
            {
                int idx1 = startIndex + lat;
                int idx2 = startIndex + lat + 1;

                // ����Ƿ�Ϊ0�Ⱦ��ߣ�lon == 0��
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

        // γ������ (ȫ����ͨ��ɫ)
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

        // ��������
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.subMeshCount = 2; // ����������

        // ������������ͨ�ߣ�
        mesh.SetIndices(mainIndices.ToArray(), MeshTopology.Lines, 0);

        // ������������0�Ⱦ��ߣ�
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
        
        // ��ȡ��ǰ��ʾ�뾶
        float actualRadius = CoordinateManager.Instance.GetBodyRadius(celestialBodyData);
        
        // �����߿�
        GenerateMeshData(actualRadius);
        
        // ������ײ��
        SphereCollider collider = GetComponent<SphereCollider>();
        if (collider != null)
        {
            collider.radius = actualRadius;
        }
    }
}