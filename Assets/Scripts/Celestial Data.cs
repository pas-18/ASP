// Celestial Data.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewCelestialData", menuName = "Celestial Data")]
public class CelestialData : ScriptableObject
{
    [System.Serializable]
    public class BodyData
    {
        public string name;
        
        public Vector3 abs_pos;  // 绝对坐标(真实坐标)

        public float abs_radius;  // 绝对半径(真实半径)

        [System.NonSerialized] public Vector3 rel_pos;  // 相对坐标(相对于焦点天体)

        [System.NonSerialized] public Vector3 display_pos;  // 修正坐标(用于渲染)

        [System.NonSerialized] public float display_radius;  // 修正半径(用于渲染)
        
        public Color color = Color.white;
        public Vector3 velocity;

        // 线框设置
        public int longitudeSegments = 24;
        public int latitudeSegments = 12;
        public float lineWidth = 0.02f;

        [System.NonSerialized] public bool isFocus;  // 是否作为焦点天体
    }

    public List<BodyData> bodies = new List<BodyData>();
}