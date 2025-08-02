// FocusManager.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FocusManager : MonoBehaviour
{
    void Update()
    {
        // �����ּ��л���������
        for (int i = 0; i < 9; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                if (i < GameManager.Instance.celestialData.bodies.Count)
                {
                    SetFocus(i);
                }
            }
        }

        // ������л�����
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                AutoWireframeSphere sphere = hit.collider.GetComponent<AutoWireframeSphere>();
                if (sphere != null)
                {
                    int index = GameManager.Instance.celestialData.bodies
                        .FindIndex(b => b.name == sphere.celestialName);

                    if (index >= 0)
                    {
                        SetFocus(index);
                    }
                }
            }
        }
    }

    void SetFocus(int index)
    {
        CoordinateManager.Instance.SetFocusBody(
            GameManager.Instance.celestialData.bodies[index]);

        // ���´�����������
        FindObjectOfType<Create>().RecreateAll();
    }

    // ������´�������
    public void RecreateAll()
    {
        // ����������������
        foreach (var body in Create.celestialbody_list)
        {
            Destroy(body);
        }
        Create.celestialbody_list.Clear();

        // ���´���
        GetComponent<Create>().Start();
    }
}