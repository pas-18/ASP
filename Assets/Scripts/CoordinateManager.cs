// CoordinateManager.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoordinateManager : MonoBehaviour
{
    public static CoordinateManager Instance;

    // ���㾫����ֵ�������˾��뿪ʼѹ����
    public const float FLOAT_PRECISION_THRESHOLD = 1000f;

    // ��ǰ��������
    private CelestialData.BodyData focusBody;

    void Awake()
    {
        Instance = this;
    }

    // ���ý�������
    public void SetFocusBody(CelestialData.BodyData body)
    {
        focusBody = body;
        focusBody.isFocus = true;
        UpdateAllPositions();
    }

    // ��ȡ��������
    public CelestialData.BodyData GetFocusBody()
    {
        return focusBody;
    }

    // �������������λ��
    public void UpdateAllPositions()
    {
        if (focusBody == null) return;

        foreach (var body in GameManager.Instance.celestialData.bodies)
        {
            if (body == focusBody)
            {
                // �����������ԭ��
                body.rel_pos = Vector3.zero;
                body.display_pos = Vector3.zero;
                body.display_radius = body.abs_radius / 600000;
            }
            else
            {
                // �������λ��
                body.rel_pos = (body.abs_pos - focusBody.abs_pos) / 600000;

                // ���㵽����ľ���
                float distance = body.rel_pos.magnitude;

                // ����Ƿ���Ҫѹ��
                if (distance > FLOAT_PRECISION_THRESHOLD)
                {
                    // ����ѹ������
                    float compressionRatio = FLOAT_PRECISION_THRESHOLD / distance;

                    // Ӧ��ѹ��
                    body.display_pos = body.rel_pos * compressionRatio;
                    body.display_radius = body.abs_radius * compressionRatio;
                }
                else
                {
                    // ����Ҫѹ��
                    body.display_pos = body.rel_pos;
                    body.display_radius = body.abs_radius;
                }
            }
        }
    }

    // ��ȡ�����ڳ����е�λ��
    public Vector3 GetBodyPosition(CelestialData.BodyData body)
    {
        return body.display_pos;
    }

    // ��ȡ�����ڳ����еİ뾶
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