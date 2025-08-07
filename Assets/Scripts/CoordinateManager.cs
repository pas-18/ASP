// CoordinateManager.cs
//透视自适应坐标系统 (Perspective Adaptive Coordinate System - PACS)
using UnityEngine;

public class CoordinateManager : MonoBehaviour
{
    public static CoordinateManager Instance;

    public const float PRECISION_THRESHOLD = 2500f;  // 压缩阈值
    public bool ensureMinViewAngle = false; // UI开关
    public float minViewAngle = 0f; // 最小视角(tan值)
    public float scaleFactor = 600000f; // 缩放因子

    // 当前焦点天体
    private CelestialData.BodyData focusBody;
    public string focusbodyname;
    private Camera mainCamera;
    public float updateInterval = 0.1f; // 每秒更新10次
    // private float lastUpdateTime;

    void Awake()
    {
        Instance = this;
        mainCamera = Camera.main;
    }

    
    void Update()
    {
        /*
        if (Time.time - lastUpdateTime > updateInterval)
        {
            UpdateAllPositions();
            lastUpdateTime = Time.time;
        }
        */
        UpdateAllPositions();
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
        Debug.Log("调用!");
        if (focusBody == null || mainCamera == null) return;

        Vector3 cameraPos = mainCamera.transform.position;

        foreach (var body in GameManager.Instance.celestialData.bodies)
        {
            // 焦点天体处理
            if (body == focusBody)
            {
                body.display_pos = Vector3.zero;
                body.display_radius = body.abs_radius / scaleFactor;
            }
            // 基本缩放
            Vector3 relPos = (body.abs_pos - focusBody.abs_pos) / scaleFactor;
            float baseRadius = body.abs_radius / scaleFactor;

            // 1. 计算天体与相机的相对位置
            Vector3 camToBody = relPos - cameraPos;
            float distanceToCam = camToBody.magnitude;

            // 2. 计算原始视角大小
            float baseViewRad = baseRadius / distanceToCam;

            // 3. 距离超过阈值时进行坐标压缩
            if (distanceToCam > PRECISION_THRESHOLD)
            {
                // 4. 视角
                if (ensureMinViewAngle)
                {
                    body.display_radius = (minViewAngle * PRECISION_THRESHOLD > baseViewRad * PRECISION_THRESHOLD)
                     ? minViewAngle * PRECISION_THRESHOLD : baseViewRad * PRECISION_THRESHOLD;
                }
                else
                {
                    body.display_radius = baseViewRad * PRECISION_THRESHOLD;
                }

                // 5. 计算修正后的位置（基于相机位置）
                Vector3 dirToBody = camToBody.normalized;
                body.display_pos = cameraPos + dirToBody * PRECISION_THRESHOLD;
            }
            // 未超过阈值，不缩放
            else
            {
                // Debug.Log(body.name + "NoOperation");
                body.display_pos = relPos;
                body.display_radius = baseRadius;
                if (ensureMinViewAngle)
                {
                    body.display_radius = (minViewAngle * distanceToCam > baseViewRad * distanceToCam)
                     ? minViewAngle * distanceToCam : baseViewRad * distanceToCam;
                }
                else
                {
                    body.display_radius = baseRadius;
                }
            }
        }

        UpdateBodyPositions();
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
        if (body == null)
        {
            Debug.Log("Null");
        }
        // Debug.Log(body.name + body.display_radius.ToString());
        return body.display_radius;
    }
}