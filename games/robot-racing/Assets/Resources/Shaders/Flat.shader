// The one lit shader of the market: a flat colour, shadows cast and received, an
// optional glow (the lamps at dusk, the antenna while a robot thinks). Lives in
// Resources so every build carries it; the Standard shader would add minutes of
// variant compiling to the web build.
Shader "Maaloomatiia/Flat"
{
    Properties
    {
        _Color ("Colour", Color) = (1, 1, 1, 1)
        _Emission ("Glow", Color) = (0, 0, 0, 0)
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" }
        LOD 200
        CGPROGRAM
        #pragma surface surf Lambert fullforwardshadows addshadow exclude_path:deferred exclude_path:prepass nolightmap nodynlightmap nodirlightmap nometa
        #pragma multi_compile_instancing
        #pragma target 3.0
        struct Input { float3 worldPos; };
        fixed4 _Color;
        fixed4 _Emission;
        void surf (Input IN, inout SurfaceOutput o)
        {
            o.Albedo = _Color.rgb;
            o.Emission = _Emission.rgb;
            o.Alpha = 1;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
