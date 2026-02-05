Shader "Game/DigBrush"
{
    Properties
    {
        _MainTex ("Brush Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
        LOD 100
        
        // Blend state to "Erase" based on alpha
        // Destination color is preserved where brush alpha is 0
        // Destination becomes transparent where brush alpha is 1
        // Algorithm: Final = (0,0,0,0) * SrcAlpha + Dest * (1-SrcAlpha)
        Blend SrcAlpha OneMinusSrcAlpha
        
        ZWrite Off
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Sample the brush texture (alpha mainly)
                fixed4 col = tex2D(_MainTex, i.uv);
                
                // We want to "Erase", so we output 0 color.
                // The Blend mode handles the rest.
                // Output Alpha must be the Brush's Alpha to control the blending factor.
                return fixed4(0, 0, 0, col.a);
            }
            ENDCG
        }
    }
}
