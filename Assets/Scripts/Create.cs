// Create.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Create : MonoBehaviour
{
    [Header("天体设置")]
    public CelestialData celestialData; // 拖入创建的数据资源
    public GameObject bodyPrefab;       // 拖入天体预制体
    public ConstructionSet constructionSet;
    public GameObject constructionPrefab; // 建筑预制体

    public static List<GameObject> celestialbody_list = new List<GameObject>();

    public void Start()
    {
        // 清空列表
        celestialbody_list.Clear();

        if (bodyPrefab == null)
        {
            Debug.LogError("未指定天体预制体！");
            return;
        }

        if (celestialData == null)
        {
            Debug.LogError("未指定天体数据！");
            return;
        }

        // 初始化坐标管理器
        CoordinateManager coordManager = gameObject.AddComponent<CoordinateManager>();

        CreateCelestialBodies();
        CreateConstructions();
    }

    void CreateCelestialBodies()
    {
        foreach (var body in celestialData.bodies)
        {
            // 获取修正位置
            Vector3 position = CoordinateManager.Instance.GetBodyPosition(body);
            
            // 实例化新天体
            GameObject newBody = Instantiate(
                bodyPrefab,
                position,
                Quaternion.identity
            );
            // Debug.Log("\nName:" + body.name.ToString() + "\nColor:" + body.color);
            // 设置父对象和名称
            newBody.transform.SetParent(transform);
            newBody.name = body.name;

            // 设置缩放
            float displayRadius = CoordinateManager.Instance.GetBodyRadius(body);
            newBody.transform.localScale = Vector3.one * displayRadius;

            // 设置颜色
            SetBodyColor(newBody, body.color);

            // 获取或添加线框球体组件
            AutoWireframeSphere wireframeSphere = newBody.GetComponent<AutoWireframeSphere>();
            if (wireframeSphere == null)
            {
                wireframeSphere = newBody.AddComponent<AutoWireframeSphere>();
            }

            // 设置线框属性
            wireframeSphere.celestialName = body.name;
            wireframeSphere.longitudeSegments = body.longitudeSegments;
            wireframeSphere.latitudeSegments = body.latitudeSegments;
            wireframeSphere.lineWidth = body.lineWidth;
            wireframeSphere.celestialBodyData = celestialData.bodies.Find(b => b.name == body.name);

            // 初始化并生成线框
            wireframeSphere.InitializeComponents();
            wireframeSphere.GenerateWireframe();

            // 添加物理属性
            AddPhysics(newBody, body.velocity);

            // 将新天体添加到全局列表
            celestialbody_list.Add(newBody);
        }
        Debug.Log($"创建了 {celestialbody_list.Count} 个天体");
    }


    // 重新创建方法
    public void RecreateAll()
    {
        // 销毁所有现有天体
        foreach (var body in celestialbody_list)
        {
            Destroy(body);
        }
        celestialbody_list.Clear();

        // 重新创建
        Start();
    }


    // 创建建筑
    void CreateConstructions()
    {
        if (constructionSet == null || constructionPrefab == null)
        {
            Debug.LogWarning("ConstructionSet or prefab not assigned!");
            return;
        }

        foreach (var construction in constructionSet.Constructions)
        {
            // 查找所属天体
            // Debug.Log("FFFFFFFF");
            GameObject celestialBody = FindCelestialBody(construction.Celestial);
            if (celestialBody == null)
            {
                Debug.LogWarning($"Celestial body '{construction.Celestial}' not found for construction '{construction.name}'");
                continue;
            }

            // 获取天体数据
            AutoWireframeSphere wireframe = celestialBody.GetComponent<AutoWireframeSphere>();
            float celestialRadius = CoordinateManager.Instance.GetBodyRadius(
                celestialData.bodies.Find(b => b.name == construction.Celestial));

            // 计算建筑在球面上的位置
            Vector3 position = CalculateSurfacePosition(
                celestialBody.transform.position,
                celestialRadius,
                construction.latitude,
                construction.longitude
            );

            // 实例化建筑
            GameObject constructionObj = Instantiate(
                constructionPrefab,
                position,
                Quaternion.identity,
                celestialBody.transform
            );

            constructionObj.layer = LayerMask.NameToLayer("Constructions");

            // 确保碰撞器启用
            SphereCollider collider = constructionObj.GetComponent<SphereCollider>();
            if (collider != null) collider.enabled = true;

            // 设置建筑属性
            constructionObj.name = construction.name;
            OrbitalConstruction orbitalConstruction = constructionObj.GetComponent<OrbitalConstruction>();
            if (orbitalConstruction != null)
            {
                orbitalConstruction.constructionName = construction.name;
                orbitalConstruction.latitude = construction.latitude;
                orbitalConstruction.longitude = construction.longitude;
                orbitalConstruction.radius = construction.radius;
                orbitalConstruction.lineWidth = construction.lineWidth;
                orbitalConstruction.color = construction.color;
                orbitalConstruction.GenerateConstructionMesh();
            }

            // 设置颜色
            SetConstructionColor(constructionObj, construction.color);
        }
        Debug.Log($"创建了 {constructionSet.Constructions.Count} 个建筑");
    }

    // 根据经纬度计算球面上的位置
    Vector3 CalculateSurfacePosition(Vector3 center, float radius, float latitude, float longitude)
    {
        // 将经纬度转换为弧度
        float latRad = latitude * Mathf.Deg2Rad;
        float lonRad = longitude * Mathf.Deg2Rad;

        // 计算球面上的位置
        float x = radius * Mathf.Cos(latRad) * Mathf.Cos(lonRad);
        float y = radius * Mathf.Sin(latRad);
        float z = radius * Mathf.Cos(latRad) * Mathf.Sin(lonRad);

        return center + new Vector3(x, y, z);
    }

    // 查找天体
    GameObject FindCelestialBody(string name)
    {
        foreach (GameObject body in celestialbody_list)
        {
            if (body.name == name)
            {
                return body;
            }
        }
        return null;
    }


    void SetBodyColor(GameObject bodyObj, Color color)
    {
        Renderer renderer = bodyObj.GetComponent<Renderer>();
        if (renderer != null)
        {
            // 确保材质存在
            if (renderer.sharedMaterial == null)
            {
                // 创建新材质实例
                Material newMaterial = new Material(Shader.Find("Custom/LatLongWireframe"));
                renderer.sharedMaterial = newMaterial;
            }

            // 设置颜色属性
            if (renderer.sharedMaterial.HasProperty("_LineColor"))
            {
                renderer.sharedMaterial.SetColor("_LineColor", color);
            }

            // 设置背景颜色为完全透明
            if (renderer.sharedMaterial.HasProperty("_BackgroundColor"))
            {
                renderer.sharedMaterial.SetColor("_BackgroundColor", new Color(0, 0, 0, 0));
            }

            // 更新线框组件中的材质引用
            AutoWireframeSphere wireframe = bodyObj.GetComponent<AutoWireframeSphere>();
            if (wireframe != null)
            {
                // 直接设置公共属性
                wireframe.wireframeMaterial = renderer.sharedMaterial;
            }
        }
    }


    void AddPhysics(GameObject body, Vector3 initialVelocity)
    {
        // 添加刚体组件
        Rigidbody rb = body.AddComponent<Rigidbody>();

        // 设置物理属性
        // rb.mass = body.transform.localScale.x * 100f; // 质量基于尺寸
        rb.useGravity = false; // 禁用Unity默认重力
        rb.interpolation = RigidbodyInterpolation.Interpolate; // 平滑移动

        // 设置初始速度
        rb.velocity = initialVelocity;

        // 添加碰撞体（如果预制体没有的话）
        if (body.GetComponent<Collider>() == null)
        {
            body.AddComponent<SphereCollider>();
        }
    }


    // 设置建筑颜色
    void SetConstructionColor(GameObject constructionObj, Color color)
    {
        Renderer renderer = constructionObj.GetComponent<Renderer>();
        if (renderer != null && renderer.sharedMaterial != null)
        {
            if (renderer.sharedMaterial.HasProperty("_LineColor"))
            {
                renderer.sharedMaterial.SetColor("_LineColor", color);
            }
        }
    }

}