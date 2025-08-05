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
        // ȷ����������ȷ�Ĳ㼶
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

        // ��������
        if (constructionMaterial == null)
        {
            constructionMaterial = new Material(Shader.Find("Custom/LatLongWireframe"));
            constructionRenderer.sharedMaterial = constructionMaterial;
        }

        // �����ײ�����������
        collider = GetComponent<SphereCollider>();
        collider.radius = radius;
        collider.isTrigger = true;
        if (collider == null)
        {
            Debug.Log("�����ײ��");
            collider = gameObject.AddComponent<SphereCollider>();
            collider.radius = radius;
            collider.isTrigger = true;
        }
    }

    public void GenerateConstructionMesh()
    {
        InitializeComponents();

        // Բ���ķֶ���
        int segments = 32;
        Vector3[] vertices = new Vector3[segments];
        int[] indices = new int[segments * 2];

        // ���ɶ���
        for (int i = 0; i < segments; i++)
        {
            float angle = 2f * Mathf.PI * i / segments;
            vertices[i] = new Vector3(
                radius * Mathf.Cos(angle),
                0,
                radius * Mathf.Sin(angle)
            );
        }

        // ��������
        for (int i = 0; i < segments; i++)
        {
            indices[i * 2] = i;
            indices[i * 2 + 1] = (i + 1) % segments;
        }

        // ��������
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.SetIndices(indices, MeshTopology.Lines, 0);

        // ���²���
        constructionMaterial.SetColor("_LineColor", color);
        constructionMaterial.SetFloat("_LineWidth", lineWidth);
        constructionMaterial.SetColor("_BackgroundColor", new Color(0, 0, 0, 0));

        // ������ײ����С
        if (collider != null)
        {
            collider.radius = radius;
        }
    }


    public void ShowConstructionInfo()
    {
        Debug.Log($"��������: {constructionName}");
        UIManager.Instance.ShowConstructionInfo(this);
        /*
        if (IsVisibleFromCamera())
        { 
            UIManager.Instance.ShowConstructionInfo(this, transform.position); 
        }
        */
    }

    // ȷ��ԲȦʼ��������������
    void Update()
    {
        if (transform.parent != null)
        {
            // ����ӽ������������ĵķ���
            Vector3 toCenter = transform.parent.position - transform.position;

            /*
            if (toCenter != Vector3.zero)
            {
                // ��ת����ʹ�䷨��ָ����������
                transform.rotation = Quaternion.LookRotation(toCenter);
            }
            Vector3 toCenter = transform.parent.position - transform.position;
            */

            if (toCenter != Vector3.zero)
            {
                Vector3 forward = Vector3.Cross(toCenter, Vector3.up);
                if (forward == Vector3.zero) // ���toCenter��upƽ��
                {
                    forward = Vector3.Cross(toCenter, Vector3.forward);
                }
                transform.rotation = Quaternion.LookRotation(forward, -toCenter.normalized);
            }
        }
    }
}