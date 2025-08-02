//CameraController.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Text;
using System;


public class CameraController : MonoBehaviour
{
    [SerializeField] private TMP_Text debugText;
    private StringBuilder sb = new StringBuilder();

    // ��������
    public string focus_body = "Kerbin"; // Ĭ�Ͻ�������
    private Vector3 focus_pos = Vector3.zero; // ����λ��

    // ������������ [r, theta, fai]
    // r: ���������ľ���
    // theta: ��ֱ�Ƕ� (0-��)��0=���Ϸ�����=���·�
    // fai: ˮƽ�Ƕ� (0-2��)��0=��ǰ��
    private Vector3 camera_data = new Vector3(50f, Mathf.PI / 4f, 0f);

    // ������Ʋ���
    public float rotationSpeed = 2f;     // ��ת�ٶ�
    public float zoomSpeed = 10f;        // �����ٶ�
    public float minDistance = 5f;       // ��С����
    public float maxDistance = 500f;     // ������

    // �����Ʊ���
    private Vector2 lastMousePosition;   // ��һ֡���λ��
    private bool isRightMouseDown = false; // �Ҽ��Ƿ���

    void Start()
    {
        // ��ʼ��Ϊ��ǰ���λ��
        lastMousePosition = Input.mousePosition;
    }

    void Update()
    {
        // 1. ���½���λ��
        UpdateFocusPosition();

        // 2. ��������������
        HandleMouseWheel();

        // 3. �����Ҽ���ת
        HandleRightMouseRotation();

        // 4. �������λ��
        UpdateCameraPosition();

        UpdateDebugText();
    }

    void UpdateFocusPosition()
    {
        // �����������壬�ҵ����������λ��
        foreach (GameObject body in Create.celestialbody_list)
        {
            if (body.name == focus_body)
            {
                focus_pos = body.transform.position;
                minDistance = (float)(1.5 * body.transform.lossyScale.x);  // ��С����������뾶�й�
                if(camera_data.x < minDistance)
                {
                    camera_data.x = minDistance;
                }
                break;
            }
        }
    }

    void HandleMouseWheel()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
        {
            // �������� +���룬���� -����
            if(scroll < 0)
            {
                camera_data.x = (float)(minDistance + (camera_data.x - 0.7 * minDistance) * Math.Pow((1 + zoomSpeed), Math.Abs(scroll)));
            }
            else
            {
                camera_data.x = (float)(minDistance + (camera_data.x - 1.25 * minDistance) * Math.Pow((1 - zoomSpeed), Math.Abs(scroll)));
            }
                // camera_data.x -= scroll * zoomSpeed;

            // ���ƾ��뷶Χ
            camera_data.x = Mathf.Clamp(camera_data.x, minDistance, maxDistance);
        }
    }

    void HandleRightMouseRotation()
    {
        // ����Ҽ�����
        if (Input.GetMouseButtonDown(1))
        {
            isRightMouseDown = true;
            lastMousePosition = Input.mousePosition;
        }

        // ����Ҽ��ͷ�
        if (Input.GetMouseButtonUp(1))
        {
            isRightMouseDown = false;
        }

        // �Ҽ�����ʱ������ת
        if (isRightMouseDown)
        {
            // ��������ƶ��� (dx, dy)
            Vector2 currentMousePosition = Input.mousePosition;
            Vector2 d_pos = currentMousePosition - lastMousePosition;
            lastMousePosition = currentMousePosition;

            // Ӧ����ת (ע�⣺��ֱ�����෴����Ϊ��Ļ����ϵY������)
            camera_data.y += d_pos.y * rotationSpeed * Time.deltaTime;
            camera_data.z -= d_pos.x * rotationSpeed * Time.deltaTime;

            // ���ƴ�ֱ�Ƕ� (0-��)
            camera_data.y = Mathf.Clamp(camera_data.y, 0.1f, Mathf.PI - 0.1f);

            // ȷ��ˮƽ�Ƕ���0-2�з�Χ��
            if (camera_data.z < 0) camera_data.z += Mathf.PI * 2;
            if (camera_data.z > Mathf.PI * 2) camera_data.z -= Mathf.PI * 2;
        }
    }

    void UpdateCameraPosition()
    {
        // ��������ת��Ϊֱ������
        // ������תֱ�����깫ʽ:
        // x = r * sin(theta) * cos(fai)
        // y = r * cos(theta)
        // z = r * sin(theta) * sin(fai)

        float r = camera_data.x;
        float theta = camera_data.y;
        float fai = camera_data.z;

        float sinTheta = Mathf.Sin(theta);
        float cosTheta = Mathf.Cos(theta);
        float cosFai = Mathf.Cos(fai);
        float sinFai = Mathf.Sin(fai);

        // �����������ڽ����λ��
        Vector3 cameraOffset = new Vector3(
            r * sinTheta * cosFai,
            r * cosTheta,
            r * sinTheta * sinFai
        );

        // �������λ��
        transform.position = focus_pos + cameraOffset;

        // ���ʼ�տ��򽹵�
        transform.LookAt(focus_pos);
    }

    // �ڳ����л��Ƶ�����Ϣ
    void UpdateDebugText()
    {
        if (debugText == null) return;

        sb.Clear();
        sb.AppendLine($"��������: {focus_body}");
        sb.AppendLine($"�������: {camera_data.x:F1}");
        sb.AppendLine($"��ֱ�Ƕ�: {camera_data.y * Mathf.Rad2Deg:F1}��");
        sb.AppendLine($"ˮƽ�Ƕ�: {camera_data.z * Mathf.Rad2Deg:F1}��");
        sb.Append($"����λ��: {focus_pos}");

        debugText.text = sb.ToString();
    }
}

