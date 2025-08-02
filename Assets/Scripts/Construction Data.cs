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
        public string Celestial;  // 所属天体
        public string name;  // 建筑名称
        [Header("坐标")]
        [Tooltip("经度 (-180-180度)")]
        [Range(-180f, 180f)] public float longitude;

        [Tooltip("纬度 (-90到90度)")]
        [Range(-90f, 90f)] public float latitude;
        public Color color = Color.white;
        public float radius = 0.01f;  // 圆形半径
        public float lineWidth = 0.75f;  // 线宽
    }

    public List<ConstructionData> Constructions = new List<ConstructionData>();
}
