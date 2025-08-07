// UIManager.cs
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    // 建筑信息UI
    public GameObject constructionInfoPanel;
    public TMP_Text constructionNameText;
    public TMP_Text constructionCoordinatesText;
    public Vector2 constructionOffset = new Vector2(20, -20); // 右下角偏移
    
    // 天体信息UI
    public GameObject celestialInfoPanel;
    public TMP_Text celestialNameText;
    public Vector2 celestialOffset = new Vector2(-20, 20); // 左下角偏移
    
    private RectTransform canvasRect;

    [Header("视角控制")]
    public Toggle viewAngleToggle;
    public Slider viewAngleSlider;
    public TMP_Text viewAngleText;
    
    
    void Start()
    {
        // 初始化视角控制
        if (viewAngleToggle != null)
        {
            viewAngleToggle.isOn = CoordinateManager.Instance.ensureMinViewAngle;
            viewAngleToggle.onValueChanged.AddListener(OnViewAngleToggleChanged);
        }
        
        if (viewAngleSlider != null)
        {
            viewAngleSlider.value = CoordinateManager.Instance.minViewAngle;
            viewAngleSlider.onValueChanged.AddListener(OnViewAngleSliderChanged);
            UpdateViewAngleText();
        }
    }

    public void OnViewAngleToggleChanged(bool isOn)
    {
        CoordinateManager.Instance.ensureMinViewAngle = isOn;
        CoordinateManager.Instance.UpdateAllPositions();
    }

    public void OnViewAngleSliderChanged(float value)
    {
        CoordinateManager.Instance.minViewAngle = value;
        UpdateViewAngleText();
        CoordinateManager.Instance.UpdateAllPositions();
    }

    private void UpdateViewAngleText()
    {
        if (viewAngleText != null)
        {
            viewAngleText.text = $"最小视角(tan值): {viewAngleSlider.value:F3}";
        }
    }

    void Awake()
    {
        Debug.Log("Awake");
        Instance = this;
        canvasRect = GetComponent<RectTransform>();
        constructionInfoPanel.SetActive(false);
        celestialInfoPanel.SetActive(false);
    }
    
    void Update()
    {
        // 更新UI位置（如果正在显示）
        if (constructionInfoPanel.activeSelf)
        {
            UpdatePanelPosition(constructionInfoPanel, Input.mousePosition, constructionOffset);
        }
        
        if (celestialInfoPanel.activeSelf)
        {
            UpdatePanelPosition(celestialInfoPanel, Input.mousePosition, celestialOffset);
        }
    }
    
    // 显示建筑信息（跟随鼠标）
    public void ShowConstructionInfo(OrbitalConstruction construction)
    {
        constructionNameText.text = construction.constructionName;
        string text_s = $"{construction.name}\n";
        if (construction.longitude > 0)
        {
            text_s += $"{construction.longitude:F2}°N\t";
        }
        else
        {
            text_s += $"{-construction.longitude:F2}°S\t";
        }
        if (construction.latitude > 0)
        {
            text_s += $" {construction.latitude:F2}°E";
        }
        else
        {
            text_s += $" {-construction.latitude:F2}°W";
        }
        constructionCoordinatesText.text = text_s;
        
        // 初始设置位置
        UpdatePanelPosition(constructionInfoPanel, Input.mousePosition, constructionOffset);
        constructionInfoPanel.SetActive(true);
        // 隐藏天体信息（如果正在显示）
        HideCelestialInfo();
    }
    
    // 显示天体信息（跟随鼠标）
    public void ShowCelestialInfo(AutoWireframeSphere celestial)
    {
        celestialNameText.text = celestial.celestialName;
        
        // 初始设置位置
        UpdatePanelPosition(celestialInfoPanel, Input.mousePosition, celestialOffset);
        celestialInfoPanel.SetActive(true);
        
        // 隐藏建筑信息（如果正在显示）
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
    
    // 更新UI面板位置（基于鼠标位置）
    private void UpdatePanelPosition(GameObject panel, Vector2 mousePosition, Vector2 offset)
    {
        Vector2 screenPoint = mousePosition;
        
        Vector2 finalPosition = screenPoint + offset;
        
        // 确保UI在屏幕内
        RectTransform panelRect = panel.GetComponent<RectTransform>();
        float width = panelRect.rect.width;
        float height = panelRect.rect.height;
        finalPosition.x = Mathf.Clamp(finalPosition.x, width / 2, Screen.width - width / 2);
        finalPosition.y = Mathf.Clamp(finalPosition.y, height / 2, Screen.height - height / 2);
        
        panelRect.position = finalPosition;
    }
}