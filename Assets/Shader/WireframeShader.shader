Shader "Custom/WireframeSphere"
{
    Properties
    {
        _WireColor ("Wire Color", Color) = (1,1,1,1)
        _BackgroundColor ("Background Color", Color) = (0,0,0,0)
        _WireWidth ("Wire Width", Range(0, 0.5)) = 0.05
    }
    
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Transparent" }
        LOD 100
        
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma geometry geom
            
            #include "UnityCG.cginc"
            
            struct appdata
            {
                float4 vertex : POSITION;
            };
            
            struct v2g
            {
                float4 vertex : SV_POSITION;
            };
            
            struct g2f
            {
                float4 vertex : SV_POSITION;
                float3 barycentric : TEXCOORD0;
            };
            
            float _WireWidth;
            fixed4 _WireColor;
            fixed4 _BackgroundColor;
            
            v2g vert (appdata v)
            {
                v2g o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                return o;
            }
            
            [maxvertexcount(3)]
            void geom(triangle v2g IN[3], inout TriangleStream<g2f> stream)
            {
                g2f o;
                
                for (int i = 0; i < 3; i++)
                {
                    o.vertex = IN[i].vertex;
                    o.barycentric = float3(0, 0, 0);
                    o.barycentric[i] = 1;
                    stream.Append(o);
                }
                
                stream.RestartStrip();
            }
            
            fixed4 frag (g2f i) : SV_Target
            {
                // 计算到边缘的距离
                float3 dists = i.barycentric;
                float minDist = min(dists.x, min(dists.y, dists.z));
                
                // 计算线宽
                float wire = 1.0 - smoothstep(_WireWidth - 0.01, _WireWidth + 0.01, minDist);
                
                // 混合颜色
                fixed4 col = lerp(_BackgroundColor, _WireColor, wire);
                return col;
            }
            ENDCG
        }
    }
}