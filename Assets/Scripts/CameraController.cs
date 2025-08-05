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

    // 焦点设置
    public string focus_body = "Kerbin"; // 默认焦点天体
    private Vector3 focus_pos = Vector3.zero; // 焦点位置

    // 相机球坐标参数 [r, theta, fai]
    // r: 相机到焦点的距离
    // theta: 垂直角度 (0-π)，0=正上方，π=正下方
    // fai: 水平角度 (0-2π)，0=正前方
    private Vector3 camera_data = new Vector3(50f, Mathf.PI / 4f, 0f);

    // 相机控制参数
    public float rotationSpeed = 2f;     // 旋转速度
    public float zoomSpeed = 10f;        // 缩放速度
    public float minDistance = 5f;       // 最小距离
    public float maxDistance = 1500f;     // 最大距离

    // 鼠标控制变量
    private Vector2 lastMousePosition;   // 上一帧鼠标位置
    private bool isRightMouseDown = false; // 右键是否按下

    void Start()
    {
        // 初始化为当前鼠标位置
        lastMousePosition = Input.mousePosition;
    }

    void Update()
    {
        // 1. 更新焦点位置
        UpdateFocusPosition();

        // 2. 处理鼠标滚轮缩放
        HandleMouseWheel();

        // 3. 处理右键旋转
        HandleRightMouseRotation();

        // 4. 更新相机位置
        UpdateCameraPosition();

        UpdateDebugText();
    }

    void UpdateFocusPosition()
    {
        focus_pos = Vector3.zero;
        
        foreach (GameObject body in Create.celestialbody_list)
        {
            if (body.name == focus_body)
            {
                minDistance = (float)(1.5 * body.transform.lossyScale.x);

                if (camera_data.x < minDistance)
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
            // 滚轮向上 +距离，向下 -距离
            if(scroll < 0)
            {
                camera_data.x = (float)(minDistance + (camera_data.x - 0.7 * minDistance) * Math.Pow((1 + zoomSpeed), Math.Abs(scroll)));
            }
            else
            {
                camera_data.x = (float)(minDistance + (camera_data.x - 1.25 * minDistance) * Math.Pow((1 - zoomSpeed), Math.Abs(scroll)));
            }
                // camera_data.x -= scroll * zoomSpeed;

            // 限制距离范围
            camera_data.x = Mathf.Clamp(camera_data.x, minDistance, maxDistance);
        }
    }

    void HandleRightMouseRotation()
    {
        // 检测右键按下
        if (Input.GetMouseButtonDown(1))
        {
            isRightMouseDown = true;
            lastMousePosition = Input.mousePosition;
        }

        // 检测右键释放
        if (Input.GetMouseButtonUp(1))
        {
            isRightMouseDown = false;
        }

        // 右键按下时处理旋转
        if (isRightMouseDown)
        {
            // 计算鼠标移动量 (dx, dy)
            Vector2 currentMousePosition = Input.mousePosition;
            Vector2 d_pos = currentMousePosition - lastMousePosition;
            lastMousePosition = currentMousePosition;

            // 应用旋转 (注意：垂直方向相反，因为屏幕坐标系Y轴向下)
            camera_data.y += d_pos.y * rotationSpeed * Time.deltaTime;
            camera_data.z -= d_pos.x * rotationSpeed * Time.deltaTime;

            // 限制垂直角度 (0-π)
            camera_data.y = Mathf.Clamp(camera_data.y, 0.1f, Mathf.PI - 0.1f);

            // 确保水平角度在0-2π范围内
            if (camera_data.z < 0) camera_data.z += Mathf.PI * 2;
            if (camera_data.z > Mathf.PI * 2) camera_data.z -= Mathf.PI * 2;
        }
    }

    void UpdateCameraPosition()
    {
        // 从球坐标转换为直角坐标
        // 球坐标转直角坐标公式:
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

        // 计算相机相对于焦点的位置
        Vector3 cameraOffset = new Vector3(
            r * sinTheta * cosFai,
            r * cosTheta,
            r * sinTheta * sinFai
        );

        // 设置相机位置
        transform.position = focus_pos + cameraOffset;

        // 相机始终看向焦点
        transform.LookAt(focus_pos);
    }

    // 在场景中绘制调试信息
    void UpdateDebugText()
    {
        if (debugText == null) return;

        sb.Clear();
        sb.AppendLine($"焦点天体: {focus_body}");
        sb.AppendLine($"相机距离: {camera_data.x:F1}");
        sb.AppendLine($"垂直角度: {camera_data.y * Mathf.Rad2Deg:F1}°");
        sb.AppendLine($"水平角度: {camera_data.z * Mathf.Rad2Deg:F1}°");
        sb.Append($"焦点位置: {focus_pos}");

        debugText.text = sb.ToString();
    }
}

