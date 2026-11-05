// Particles, for the Universal Render Pipeline: a sprite tinted by the particle's own colour,
// additive for sparks and confetti (_Dst = One), alpha-blended for dust and tokens (_Dst = OneMinusSrcAlpha).
// One small shader instead of URP's particle set, so the web build stays small.
Shader "Maaloomatiia/Spark"
{
    Properties
    {
        _MainTex ("Sprite", 2D) = "white" {}
        _Color ("Tint", Color) = (1, 1, 1, 1)
        [Enum(UnityEngine.Rendering.BlendMode)] _Dst ("Blend", Float) = 1
    }
    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" "IgnoreProjector" = "True" "PreviewType" = "Plane" }
        Blend SrcAlpha [_Dst]
        ZWrite Off
        Cull Off
        Pass
        {
            Name "Particles"
            Tags { "LightMode" = "UniversalForward" }
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);
            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                half4 _Color;
                float _Dst;
            CBUFFER_END
            struct Attributes { float4 positionOS : POSITION; float2 uv : TEXCOORD0; half4 color : COLOR; };
            struct Varyings { float4 positionCS : SV_POSITION; float2 uv : TEXCOORD0; half4 color : COLOR; };
            Varyings vert(Attributes i)
            {
                Varyings o;
                o.positionCS = TransformObjectToHClip(i.positionOS.xyz);
                o.uv = TRANSFORM_TEX(i.uv, _MainTex);
                o.color = i.color * _Color;
                return o;
            }
            half4 frag(Varyings i) : SV_Target
            {
                half4 s = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
                return s * i.color;
            }
            ENDHLSL
        }
    }
    FallBack Off
}
