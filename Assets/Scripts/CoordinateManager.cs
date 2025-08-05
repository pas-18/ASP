// CoordinateManager.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoordinateManager : MonoBehaviour
{
    public static CoordinateManager Instance;

    public const float FLOAT_PRECISION_THRESHOLD = 1000f;  // 压缩阈值
    public bool ensureMinViewAngle = false; // UI开关
    public float minViewAngle = 0f; // 最小视角(度)

    // 当前焦点天体
    private CelestialData.BodyData focusBody;
    public string focusbodyname;
    private Camera mainCamera;
    public float updateInterval = 0.1f; // 每秒更新10次
    private float lastUpdateTime;

    void Awake()
    {
        Instance = this;
        mainCamera = Camera.main;
    }
    
    void Update()
    {
        if (Time.time - lastUpdateTime > updateInterval)
        {
            UpdateAllPositions();
            lastUpdateTime = Time.time;
        }
    }

    // 设置焦点天体
    public void SetFocusBody(CelestialData.BodyData body)
    {
        focusBody = body;
        focusBody.isFocus = true;
        focusbodyname = focusBody.name;
        UpdateAllPositions();
    }

    // 获取焦点天体
    public CelestialData.BodyData GetFocusBody()
    {
        return focusBody;
    }

    // 更新所有天体的位置
    public void UpdateAllPositions()
    {
        if (focusBody == null || mainCamera == null) return;

        float scaleFactor = 600000; // 缩放基准600,000
        Vector3 cameraPos = mainCamera.transform.position;
        
        foreach (var body in GameManager.Instance.celestialData.bodies)
        {
            Debug.Log(body.name);
             // 焦点天体始终位于原点
            if (body == focusBody)
            {
                body.display_pos = Vector3.zero;
                body.display_radius = body.abs_radius / scaleFactor;
                continue;
            }
            // 基本缩放
            Vector3 relPos = (body.abs_pos - focusBody.abs_pos) / scaleFactor;
            float baseRadius = body.abs_radius / scaleFactor;

            // 压缩处理
            float distance = relPos.magnitude;
            if (distance > FLOAT_PRECISION_THRESHOLD)
            {
                Debug.Log("FLOAT_PRECISION_THRESHOLD:"+body.name);
                float compressionRatio = FLOAT_PRECISION_THRESHOLD / distance;
                body.display_pos = relPos * compressionRatio;
                body.display_radius = baseRadius * compressionRatio;
            }
            else
            {
                Debug.Log("NoOperation:" + body.name);
                body.display_pos = relPos;
                body.display_radius = baseRadius;
            }

            // 视角保证
            if (ensureMinViewAngle)
            {
                EnsureMinViewAngle(body, cameraPos);
            }
        }

        UpdateBodyPositions();
    }


    private void EnsureMinViewAngle(CelestialData.BodyData body, Vector3 cameraPos)
    {
        // 计算天体到相机的向量
        Vector3 toBody = body.display_pos - cameraPos;
        float distanceToCamera = toBody.magnitude;
        
        // 避免除以零
        if (distanceToCamera < 0.001f) return;
        
        // 计算当前视角(弧度)
        float currentAngleRad = Mathf.Atan2(body.display_radius, distanceToCamera);
        float currentAngleDeg = currentAngleRad * Mathf.Rad2Deg;
        float minAngleRad = minViewAngle * Mathf.Deg2Rad;
        
        // 如果当前视角小于最小视角
        if (currentAngleDeg < minViewAngle)
        {
            // 计算所需半径以保证最小视角
            float requiredRadius = distanceToCamera * Mathf.Tan(minAngleRad);
            body.display_radius = requiredRadius;
        }
    }


    // 更新游戏对象位置
    private void UpdateBodyPositions()
    {
        foreach (var bodyData in GameManager.Instance.celestialData.bodies)
        {
            GameObject bodyObj = Create.celestialbody_list.Find(b => b.name == bodyData.name);
            if (bodyObj != null)
            {
                bodyObj.transform.position = bodyData.display_pos;
                bodyObj.transform.localScale = Vector3.one * bodyData.display_radius;

                // 更新线框
                AutoWireframeSphere wireframe = bodyObj.GetComponent<AutoWireframeSphere>();
                if (wireframe != null)
                {
                    wireframe.UpdateWireframe();
                }
            }
        }
    }

    // 获取天体在场景中的位置
    public Vector3 GetBodyPosition(CelestialData.BodyData body)
    {
        return body.display_pos;
    }

    // 获取天体在场景中的半径
    public float GetBodyRadius(CelestialData.BodyData body)
    {
        if(body == null)
        {
            Debug.Log("Null");
        }
        // Debug.Log(body.name + body.display_radius.ToString());
        return body.display_radius;
    }
}