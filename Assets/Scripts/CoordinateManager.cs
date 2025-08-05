// CoordinateManager.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoordinateManager : MonoBehaviour
{
    public static CoordinateManager Instance;

    public const float FLOAT_PRECISION_THRESHOLD = 1000f;  // ѹ����ֵ
    public bool ensureMinViewAngle = false; // UI����
    public float minViewAngle = 0f; // ��С�ӽ�(��)

    // ��ǰ��������
    private CelestialData.BodyData focusBody;
    public string focusbodyname;
    private Camera mainCamera;

    void Awake()
    {
        Instance = this;
        mainCamera = Camera.main;
    }

    // ���ý�������
    public void SetFocusBody(CelestialData.BodyData body)
    {
        focusBody = body;
        focusBody.isFocus = true;
        focusbodyname = focusBody.name;
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
        if (focusBody == null || mainCamera == null) return;

        float scaleFactor = 600000; // ���Ż�׼600,000
        Vector3 cameraPos = mainCamera.transform.position;
        
        foreach (var body in GameManager.Instance.celestialData.bodies)
        {
            Debug.Log(body.name);
            // ��������
            Vector3 relPos = (body.abs_pos - focusBody.abs_pos) / scaleFactor;
            float baseRadius = body.abs_radius / scaleFactor;

            // ѹ������
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

            // �ӽǱ�֤
            if (ensureMinViewAngle)
            {
                EnsureMinViewAngle(body, cameraPos);
            }
        }

        UpdateBodyPositions();
    }


    private void EnsureMinViewAngle(CelestialData.BodyData body, Vector3 cameraPos)
    {
        // �������嵽���������
        Vector3 toBody = body.display_pos - cameraPos;
        float distanceToCamera = toBody.magnitude;
        
        // ���������
        if (distanceToCamera < 0.001f) return;
        
        // ���㵱ǰ�ӽ�(����)
        float currentAngleRad = Mathf.Atan2(body.display_radius, distanceToCamera);
        float currentAngleDeg = currentAngleRad * Mathf.Rad2Deg;
        float minAngleRad = minViewAngle * Mathf.Deg2Rad;
        
        // �����ǰ�ӽ�С����С�ӽ�
        if (currentAngleDeg < minViewAngle)
        {
            // ��������뾶�Ա�֤��С�ӽ�
            float requiredRadius = distanceToCamera * Mathf.Tan(minAngleRad);
            body.display_radius = requiredRadius;
        }
    }


    // ������Ϸ����λ��
    private void UpdateBodyPositions()
    {
        foreach (var bodyData in GameManager.Instance.celestialData.bodies)
        {
            GameObject bodyObj = Create.celestialbody_list.Find(b => b.name == bodyData.name);
            if (bodyObj != null)
            {
                bodyObj.transform.position = bodyData.display_pos;
                bodyObj.transform.localScale = Vector3.one * bodyData.display_radius;

                // �����߿�
                AutoWireframeSphere wireframe = bodyObj.GetComponent<AutoWireframeSphere>();
                if (wireframe != null)
                {
                    wireframe.UpdateWireframe();
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