using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class BodySelector : MonoBehaviour
{
    public TMP_Dropdown dropdown;
    public TMP_Text debugText; // 用于调试的文本组件（可选）

    void Start()
    {
        // 确保有Dropdown引用
        if (dropdown == null)
        {
            Debug.LogError("Dropdown 未分配！");
            return;
        }

        // 手动绑定事件
        dropdown.onValueChanged.AddListener(OnBodySelected);

        // 开始协程初始化
        StartCoroutine(InitializeAfterDelay());
    }

    IEnumerator InitializeAfterDelay()
    {
        // 等待两帧确保所有对象已创建
        yield return null;
        yield return null;

        // 检查天体列表
        if (Create.celestialbody_list == null)
        {
            UpdateDebugText("天体列表为null");
            Debug.LogError("天体列表为null");
            yield break;
        }

        if (Create.celestialbody_list.Count == 0)
        {
            UpdateDebugText("天体列表为空");
            Debug.LogWarning("天体列表为空");
            yield break;
        }

        // 创建选项列表
        List<TMP_Dropdown.OptionData> options = new List<TMP_Dropdown.OptionData>();

        foreach (GameObject body in Create.celestialbody_list)
        {
            if (body != null)
            {
                options.Add(new TMP_Dropdown.OptionData(body.name));
            }
            else
            {
                Debug.LogWarning("发现空的天体对象");
            }
        }

        if (options.Count == 0)
        {
            UpdateDebugText("选项列表为空");
            Debug.LogWarning("选项列表为空");
            yield break;
        }

        // 填充下拉菜单
        dropdown.ClearOptions();
        dropdown.AddOptions(options);
        dropdown.value = 0;
        dropdown.RefreshShownValue();

        UpdateDebugText($"成功添加 {options.Count} 个选项");
        Debug.Log($"成功添加 {options.Count} 个选项");
    }

    void UpdateDebugText(string message)
    {
        if (debugText != null)
        {
            debugText.text = message;
        }
    }

    public void OnBodySelected(int index)
    {
        if (index >= 0 && index < Create.celestialbody_list.Count)
        {
            GameObject selectedBody = Create.celestialbody_list[index];
            if (selectedBody != null)
            {
                CameraController cameraController = Camera.main.GetComponent<CameraController>();
                if (cameraController != null)
                {
                    cameraController.focus_body = selectedBody.name;
                    Debug.Log($"切换焦点到: {selectedBody.name}");
                }
                else
                {
                    Debug.LogError("未找到 CameraController");
                }
            }
        }
    }
}