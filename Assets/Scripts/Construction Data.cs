// Construction Data.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewConstructionData", menuName = "Construction Data")]
public class ConstructionSet : ScriptableObject
{
    [System.Serializable]
    public class ConstructionData
    {
        public string Celestial;  // ��������
        public string name;  // ��������
        [Header("����")]
        [Tooltip("���� (-180-180��)")]
        [Range(-180f, 180f)] public float longitude;

        [Tooltip("γ�� (-90��90��)")]
        [Range(-90f, 90f)] public float latitude;
        public Color color = Color.white;
        public float radius = 0.01f;  // Բ�ΰ뾶
        public float lineWidth = 0.75f;  // �߿�
    }

    public List<ConstructionData> Constructions = new List<ConstructionData>();
}
