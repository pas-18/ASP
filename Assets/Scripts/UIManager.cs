// UIManager.cs
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
    public Vector2 celestialOffset = new Vector2(-20, 20); // ���½�ƫ��
    
    private RectTransform canvasRect;
    
    void Awake()
    {
        Instance = this;
        canvasRect = GetComponent<RectTransform>();
        constructionInfoPanel.SetActive(false);
        celestialInfoPanel.SetActive(false);
    }
    
    void Update()
    {
        // ����UIλ�ã����������ʾ��
        if (constructionInfoPanel.activeSelf)
        {
            UpdatePanelPosition(constructionInfoPanel, Input.mousePosition, constructionOffset);
        }
        
        if (celestialInfoPanel.activeSelf)
        {
            UpdatePanelPosition(celestialInfoPanel, Input.mousePosition, celestialOffset);
        }
    }
    
    // ��ʾ������Ϣ��������꣩
    public void ShowConstructionInfo(OrbitalConstruction construction)
    {
        constructionNameText.text = construction.constructionName;
        string text_s = $"{construction.name}\n";
        if (construction.longitude > 0)
        {
            text_s += $"{construction.longitude:F2}��N\t";
        }
        else
        {
            text_s += $"{-construction.longitude:F2}��S\t";
        }
        if (construction.latitude > 0)
        {
            text_s += $" {construction.latitude:F2}��E";
        }
        else
        {
            text_s += $" {-construction.latitude:F2}��W";
        }
        constructionCoordinatesText.text = text_s;
        
        // ��ʼ����λ��
        UpdatePanelPosition(constructionInfoPanel, Input.mousePosition, constructionOffset);
        constructionInfoPanel.SetActive(true);
        
        // ����������Ϣ�����������ʾ��
        HideCelestialInfo();
    }
    
    // ��ʾ������Ϣ��������꣩
    public void ShowCelestialInfo(AutoWireframeSphere celestial)
    {
        celestialNameText.text = celestial.celestialName;
        
        // ��ʼ����λ��
        UpdatePanelPosition(celestialInfoPanel, Input.mousePosition, celestialOffset);
        celestialInfoPanel.SetActive(true);
        
        // ���ؽ�����Ϣ�����������ʾ��
        HideConstructionInfo();
    }
    
    public void HideConstructionInfo()
    {
        constructionInfoPanel.SetActive(false);
    }
    
    public void HideCelestialInfo()
    {
        celestialInfoPanel.SetActive(false);
    }
    
    // ����UI���λ�ã��������λ�ã�
    private void UpdatePanelPosition(GameObject panel, Vector2 mousePosition, Vector2 offset)
    {
        Vector2 screenPoint = mousePosition;
        
        Vector2 finalPosition = screenPoint + offset;
        
        // ȷ��UI����Ļ��
        RectTransform panelRect = panel.GetComponent<RectTransform>();
        float width = panelRect.rect.width;
        float height = panelRect.rect.height;
        finalPosition.x = Mathf.Clamp(finalPosition.x, width / 2, Screen.width - width / 2);
        finalPosition.y = Mathf.Clamp(finalPosition.y, height / 2, Screen.height - height / 2);
        
        panelRect.position = finalPosition;
    }
}