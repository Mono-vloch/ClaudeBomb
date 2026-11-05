// The sky, for the Universal Render Pipeline: a big sphere seen from inside, shaded from
// the top colour down to the horizon and on to the ground colour. Day on the market,
// dusk on the cover. Drawn first, never fogged, never lit.
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
        Tags { "Queue" = "Background" "RenderType" = "Background" "RenderPipeline" = "UniversalPipeline" "PreviewType" = "Skybox" }
        Cull Front
        ZWrite Off
        Pass
        {
            Name "Sky"
            Tags { "LightMode" = "UniversalForward" }
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            CBUFFER_START(UnityPerMaterial)
                half4 _Top, _Horizon, _Bottom;
            CBUFFER_END
            struct Attributes { float4 positionOS : POSITION; };
            struct Varyings { float4 positionCS : SV_POSITION; float h : TEXCOORD0; };
            Varyings vert(Attributes i)
            {
                Varyings o;
                o.positionCS = TransformObjectToHClip(i.positionOS.xyz);
                o.h = i.positionOS.y * 2.0; // the sphere mesh has radius 0.5: -1 at the bottom, 1 at the top
                return o;
            }
            half4 frag(Varyings i) : SV_Target
            {
                float h = i.h;
                return h >= 0 ? lerp(_Horizon, _Top, pow(saturate(h), 0.55)) : lerp(_Horizon, _Bottom, pow(saturate(-h), 0.4));
            }
            ENDHLSL
        }
    }
    FallBack Off
}
