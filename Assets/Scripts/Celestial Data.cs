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
        
        public Vector3 abs_pos;  // ��������(��ʵ����)

        public float abs_radius;  // ���԰뾶(��ʵ�뾶)

        [System.NonSerialized] public Vector3 rel_pos;  // �������(����ڽ�������)

        [System.NonSerialized] public Vector3 display_pos;  // ��������(������Ⱦ)

        [System.NonSerialized] public float display_radius;  // �����뾶(������Ⱦ)
        
        public Color color = Color.white;
        public Vector3 velocity;

        // �߿�����
        public int longitudeSegments = 24;
        public int latitudeSegments = 12;
        public float lineWidth = 0.02f;

        [System.NonSerialized] public bool isFocus;  // �Ƿ���Ϊ��������
    }

    public List<BodyData> bodies = new List<BodyData>();
}