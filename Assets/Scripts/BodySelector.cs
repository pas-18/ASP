using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class BodySelector : MonoBehaviour
{
    public TMP_Dropdown dropdown;
    public TMP_Text debugText; // ���ڵ��Ե��ı��������ѡ��

    void Start()
    {
        // ȷ����Dropdown����
        if (dropdown == null)
        {
            Debug.LogError("Dropdown δ���䣡");
            return;
        }

        // �ֶ����¼�
        dropdown.onValueChanged.AddListener(OnBodySelected);

        // ��ʼЭ�̳�ʼ��
        StartCoroutine(InitializeAfterDelay());
    }

    IEnumerator InitializeAfterDelay()
    {
        // �ȴ���֡ȷ�����ж����Ѵ���
        yield return null;
        yield return null;

        // ��������б�
        if (Create.celestialbody_list == null)
        {
            UpdateDebugText("�����б�Ϊnull");
            Debug.LogError("�����б�Ϊnull");
            yield break;
        }

        if (Create.celestialbody_list.Count == 0)
        {
            UpdateDebugText("�����б�Ϊ��");
            Debug.LogWarning("�����б�Ϊ��");
            yield break;
        }

        // ����ѡ���б�
        List<TMP_Dropdown.OptionData> options = new List<TMP_Dropdown.OptionData>();

        foreach (GameObject body in Create.celestialbody_list)
        {
            if (body != null)
            {
                options.Add(new TMP_Dropdown.OptionData(body.name));
            }
            else
            {
                Debug.LogWarning("���ֿյ��������");
            }
        }

        if (options.Count == 0)
        {
            UpdateDebugText("ѡ���б�Ϊ��");
            Debug.LogWarning("ѡ���б�Ϊ��");
            yield break;
        }

        // ��������˵�
        dropdown.ClearOptions();
        dropdown.AddOptions(options);
        dropdown.value = 0;
        dropdown.RefreshShownValue();

        UpdateDebugText($"�ɹ���� {options.Count} ��ѡ��");
        Debug.Log($"�ɹ���� {options.Count} ��ѡ��");
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
                    Debug.Log($"�л����㵽: {selectedBody.name}");
                }
                else
                {
                    Debug.LogError("δ�ҵ� CameraController");
                }
            }
        }
    }
}