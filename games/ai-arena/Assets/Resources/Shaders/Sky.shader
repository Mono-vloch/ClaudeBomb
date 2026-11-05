// The sky: a big sphere seen from inside, shaded from the top colour down to
// the horizon and on to the ground colour. Day for the market, dusk on the cover.
Shader "Maaloomatiia/Sky"
{
    Properties
    {
        _Top ("Top", Color) = (0.44, 0.70, 0.92, 1)
        _Horizon ("Horizon", Color) = (0.89, 0.95, 0.98, 1)
        _Bottom ("Bottom", Color) = (0.73, 0.85, 0.75, 1)
    }
    SubShader
    {
        Tags { "Queue" = "Background" "RenderType" = "Background" "PreviewType" = "Skybox" }
        Cull Front
        ZWrite Off
        Fog { Mode Off }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            fixed4 _Top, _Horizon, _Bottom;
            struct v2f { float4 pos : SV_POSITION; float h : TEXCOORD0; };
            v2f vert (appdata_base v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.h = v.vertex.y * 2.0; // the sphere mesh has radius 0.5: -1 at the bottom, 1 at the top
                return o;
            }
            fixed4 frag (v2f i) : SV_Target
            {
                float h = i.h;
                return h >= 0 ? lerp(_Horizon, _Top, pow(saturate(h), 0.55)) : lerp(_Horizon, _Bottom, pow(saturate(-h), 0.4));
            }
            ENDCG
        }
    }
}
