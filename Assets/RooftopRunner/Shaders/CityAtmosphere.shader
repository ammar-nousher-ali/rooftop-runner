Shader "RooftopRunner/CityAtmosphere"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _Sky ("Sky mode", Float) = 0
        _Windows ("Warm window glow", Float) = 1
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "CanUseSpriteAtlas"="True" }
        Cull Off Lighting Off ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; fixed4 color : COLOR; };
            struct v2f { float4 vertex : SV_POSITION; float2 uv : TEXCOORD0; fixed4 color : COLOR; };
            sampler2D _MainTex;
            fixed4 _Color;
            float _Sky, _Windows, _RunnerNight;
            v2f vert(appdata v)
            {
                v2f o; o.vertex = UnityObjectToClipPos(v.vertex); o.uv = v.uv; o.color = v.color * _Color; return o;
            }
            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 tex = tex2D(_MainTex, i.uv);
                float3 day = tex.rgb * i.color.rgb;
                float lum = dot(day, float3(.299,.587,.114));
                float3 night = lerp(day * float3(.32,.42,.70), float3(.045,.07,.16) + lum * float3(.08,.12,.24), _Sky);
                // Preserve and lift painted amber windows instead of placing floating lights.
                float warm = smoothstep(.10,.38,tex.r-tex.b) * smoothstep(.25,.62,tex.r) * smoothstep(.12,.35,tex.g);
                night = lerp(night, saturate(day * float3(1.6,1.35,1.05)), warm * _Windows * (1-_Sky));
                return fixed4(lerp(day,night,saturate(_RunnerNight)),tex.a*i.color.a);
            }
            ENDCG
        }
    }
}
