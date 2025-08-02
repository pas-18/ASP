// UIManager.cs
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    // ������ϢUI
    public GameObject constructionInfoPanel;
    public TMP_Text constructionNameText;
    public TMP_Text constructionCoordinatesText;
    public Vector2 constructionOffset = new Vector2(20, -20); // ���½�ƫ��

    // ������ϢUI
    public GameObject celestialInfoPanel;
    public TMP_Text celestialNameText;
    public Vector2 celestialOffset = new Vector2(-20, 20); // ���Ͻ�ƫ��

    private RectTransform canvasRect;
    private Camera mainCamera;

    void Awake()
    {
        Instance = this;
        mainCamera = Camera.main;
        canvasRect = GetComponent<RectTransform>();
        constructionInfoPanel.SetActive(false);
        celestialInfoPanel.SetActive(false);
    }

    // ��ʾ������Ϣ
    public void ShowConstructionInfo(OrbitalConstruction construction, Vector3 worldPosition)
    {
        constructionNameText.text = construction.constructionName;
        constructionCoordinatesText.text = $"����: {construction.longitude:F2}��\nγ��: {construction.latitude:F2}��";
        UpdatePanelPosition(constructionInfoPanel, worldPosition, constructionOffset);
        constructionInfoPanel.SetActive(true);
    }

    // ��ʾ������Ϣ
    public void ShowCelestialInfo(AutoWireframeSphere celestial, Vector3 worldPosition)
    {
        celestialNameText.text = celestial.celestialName;
        UpdatePanelPosition(celestialInfoPanel, worldPosition, celestialOffset);
        celestialInfoPanel.SetActive(true);
    }

    public void HideConstructionInfo()
    {
        constructionInfoPanel.SetActive(false);
    }

    public void HideCelestialInfo()
    {
        celestialInfoPanel.SetActive(false);
    }

    // ����UI���λ��
    private void UpdatePanelPosition(GameObject panel, Vector3 worldPosition, Vector2 offset)
    {
        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(mainCamera, worldPosition);
        Vector2 finalPosition = screenPoint + offset;

        // ��������Ļ��
        RectTransform panelRect = panel.GetComponent<RectTransform>();
        float width = panelRect.rect.width;
        float height = panelRect.rect.height;

        finalPosition.x = Mathf.Clamp(finalPosition.x, width / 2, Screen.width - width / 2);
        finalPosition.y = Mathf.Clamp(finalPosition.y, height / 2, Screen.height - height / 2);

        panelRect.position = finalPosition;
    }
}