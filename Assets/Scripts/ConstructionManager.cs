// ConstructionManager.cs
// OrbitalConstruction
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class OrbitalConstruction : MonoBehaviour
{
    public string constructionName;
    public float latitude;
    public float longitude;


    public float radius = 0.1f;
    public float lineWidth = 0.02f;
    public Color color = Color.white;

    private Mesh mesh;
    private Renderer constructionRenderer;
    private Material constructionMaterial;
    private SphereCollider collider;


    void Start()
    {
        InitializeComponents();
        GenerateConstructionMesh();
        // 确保物体在正确的层级
        gameObject.layer = LayerMask.NameToLayer("Constructions");
    }

    public void InitializeComponents()
    {
        if (mesh == null)
        {
            mesh = new Mesh();
            mesh.name = "ConstructionCircle";
            GetComponent<MeshFilter>().mesh = mesh;
        }

        constructionRenderer = GetComponent<Renderer>();

        // 创建材质
        if (constructionMaterial == null)
        {
            constructionMaterial = new Material(Shader.Find("Custom/LatLongWireframe"));
            constructionRenderer.sharedMaterial = constructionMaterial;
        }

        // 添加碰撞器用于鼠标检测
        collider = GetComponent<SphereCollider>();
        collider.radius = radius;
        collider.isTrigger = true;
        if (collider == null)
        {
            Debug.Log("添加碰撞器");
            collider = gameObject.AddComponent<SphereCollider>();
            collider.radius = radius;
            collider.isTrigger = true;
        }
    }

    public void GenerateConstructionMesh()
    {
        InitializeComponents();

        // 圆环的分段数
        int segments = 32;
        Vector3[] vertices = new Vector3[segments];
        int[] indices = new int[segments * 2];

        // 生成顶点
        for (int i = 0; i < segments; i++)
        {
            float angle = 2f * Mathf.PI * i / segments;
            vertices[i] = new Vector3(
                radius * Mathf.Cos(angle),
                0,
                radius * Mathf.Sin(angle)
            );
        }

        // 生成索引
        for (int i = 0; i < segments; i++)
        {
            indices[i * 2] = i;
            indices[i * 2 + 1] = (i + 1) % segments;
        }

        // 更新网格
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.SetIndices(indices, MeshTopology.Lines, 0);

        // 更新材质
        constructionMaterial.SetColor("_LineColor", color);
        constructionMaterial.SetFloat("_LineWidth", lineWidth);
        constructionMaterial.SetColor("_BackgroundColor", new Color(0, 0, 0, 0));

        // 更新碰撞器大小
        if (collider != null)
        {
            collider.radius = radius;
        }
    }


    public void ShowConstructionInfo()
    {
        Debug.Log($"建筑触发: {constructionName}");
        UIManager.Instance.ShowConstructionInfo(this);
        /*
        if (IsVisibleFromCamera())
        { 
            UIManager.Instance.ShowConstructionInfo(this, transform.position); 
        }
        */
    }

    // 确保圆圈始终面向天体中心
    void Update()
    {
        if (transform.parent != null)
        {
            // 计算从建筑到天体中心的方向
            Vector3 toCenter = transform.parent.position - transform.position;

            /*
            if (toCenter != Vector3.zero)
            {
                // 旋转建筑使其法线指向天体中心
                transform.rotation = Quaternion.LookRotation(toCenter);
            }
            Vector3 toCenter = transform.parent.position - transform.position;
            */

            if (toCenter != Vector3.zero)
            {
                Vector3 forward = Vector3.Cross(toCenter, Vector3.up);
                if (forward == Vector3.zero) // 如果toCenter和up平行
                {
                    forward = Vector3.Cross(toCenter, Vector3.forward);
                }
                transform.rotation = Quaternion.LookRotation(forward, -toCenter.normalized);
            }
        }
    }
}