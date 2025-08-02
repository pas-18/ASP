// CoordinateManager.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoordinateManager : MonoBehaviour
{
    public static CoordinateManager Instance;

    // 浮点精度阈值（超过此距离开始压缩）
    public const float FLOAT_PRECISION_THRESHOLD = 1000f;

    // 当前焦点天体
    private CelestialData.BodyData focusBody;

    void Awake()
    {
        Instance = this;
    }

    // 设置焦点天体
    public void SetFocusBody(CelestialData.BodyData body)
    {
        focusBody = body;
        focusBody.isFocus = true;
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
        if (focusBody == null) return;

        foreach (var body in GameManager.Instance.celestialData.bodies)
        {
            if (body == focusBody)
            {
                // 焦点天体放在原点
                body.rel_pos = Vector3.zero;
                body.display_pos = Vector3.zero;
                body.display_radius = body.abs_radius / 600000;
            }
            else
            {
                // 计算相对位置
                body.rel_pos = (body.abs_pos - focusBody.abs_pos) / 600000;

                // 计算到焦点的距离
                float distance = body.rel_pos.magnitude;

                // 检查是否需要压缩
                if (distance > FLOAT_PRECISION_THRESHOLD)
                {
                    // 计算压缩比例
                    float compressionRatio = FLOAT_PRECISION_THRESHOLD / distance;

                    // 应用压缩
                    body.display_pos = body.rel_pos * compressionRatio;
                    body.display_radius = body.abs_radius * compressionRatio;
                }
                else
                {
                    // 不需要压缩
                    body.display_pos = body.rel_pos;
                    body.display_radius = body.abs_radius;
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