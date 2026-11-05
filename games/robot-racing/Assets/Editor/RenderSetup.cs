using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

// Robot Racing > Set up URP: the one render pipeline asset and its renderer
// (Assets/Settings), assigned in Graphics and in every quality level, with the
// settings that suit the desktop and the web alike: HDR (the bloom needs it), no
// MSAA (the camera uses SMAA instead, cheaper on the web), soft shadows from one
// 2048 map in two cascades over 60 units, four lamps per object, the colour
// grading in HDR. Linear colour, as in Claude Bomb. Safe to run again.
public static class RenderSetup
{
    const string Dir = "Assets/Settings";
    const string RendererPath = Dir + "/RobotRacing_Renderer.asset";
    const string AssetPath = Dir + "/RobotRacing_RPAsset.asset";

    [MenuItem("Robot Racing/Set up URP")]
    public static void Run()
    {
        if (!AssetDatabase.IsValidFolder(Dir)) AssetDatabase.CreateFolder("Assets", "Settings");

        var renderer = AssetDatabase.LoadAssetAtPath<UniversalRendererData>(RendererPath);
        if (renderer == null)
        {
            renderer = ScriptableObject.CreateInstance<UniversalRendererData>();
            AssetDatabase.CreateAsset(renderer, RendererPath);
        }
        renderer.renderingMode = RenderingMode.Forward;
        renderer.depthPrimingMode = DepthPrimingMode.Disabled;
        renderer.copyDepthMode = CopyDepthMode.AfterOpaques;
        if (renderer.postProcessData == null)
            renderer.postProcessData = AssetDatabase.LoadAssetAtPath<PostProcessData>("Packages/com.unity.render-pipelines.universal/Runtime/Data/PostProcessData.asset");
        EditorUtility.SetDirty(renderer);

        var asset = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(AssetPath);
        if (asset == null)
        {
            asset = UniversalRenderPipelineAsset.Create(renderer);
            AssetDatabase.CreateAsset(asset, AssetPath);
        }
        asset.supportsHDR = true;
        asset.hdrColorBufferPrecision = HDRColorBufferPrecision._32Bits;
        asset.msaaSampleCount = 1;
        asset.renderScale = 1f;
        asset.supportsCameraDepthTexture = false;
        asset.supportsCameraOpaqueTexture = false;
        asset.mainLightShadowmapResolution = 2048;
        asset.shadowDistance = 60f;
        asset.shadowCascadeCount = 2;
        asset.cascade2Split = 0.3f;
        asset.shadowDepthBias = 1f;
        asset.shadowNormalBias = 1f;
        // The light and shadow switches are read-only properties in URP 17: set the
        // serialized fields, and fail loudly if a name has moved rather than skip it.
        var so = new SerializedObject(asset);
        Set(so, "m_MainLightRenderingMode", (int)LightRenderingMode.PerPixel);
        Set(so, "m_MainLightShadowsSupported", true);
        Set(so, "m_AdditionalLightsRenderingMode", (int)LightRenderingMode.PerPixel);
        Set(so, "m_AdditionalLightsPerObjectLimit", 4);
        Set(so, "m_AdditionalLightShadowsSupported", false);
        Set(so, "m_SoftShadowsSupported", true);
        so.ApplyModifiedPropertiesWithoutUndo();
        asset.colorGradingMode = ColorGradingMode.HighDynamicRange;
        asset.colorGradingLutSize = 32;
        asset.useSRPBatcher = true;
        asset.supportsDynamicBatching = false;
        EditorUtility.SetDirty(asset);

        // Graphics, then every quality level, the way URP's own converter does it.
        GraphicsSettings.defaultRenderPipeline = asset;
        int current = QualitySettings.GetQualityLevel();
        for (int i = 0; i < QualitySettings.names.Length; i++)
        {
            QualitySettings.SetQualityLevel(i, false);
            QualitySettings.renderPipeline = asset;
        }
        QualitySettings.SetQualityLevel(current, false);
        PlayerSettings.colorSpace = ColorSpace.Linear;
        AssetDatabase.SaveAssets();

        var rows = new System.Collections.Generic.List<string>();
        for (int i = 0; i < QualitySettings.names.Length; i++)
        {
            var a = QualitySettings.GetRenderPipelineAssetAt(i);
            rows.Add($"{QualitySettings.names[i]}={(a == null ? "inherits" : a.name)}");
        }
        var g = GraphicsSettings.defaultRenderPipeline;
        Debug.Log($"[RenderSetup] graphics={(g == null ? "NULL" : g.name)}; quality: {string.Join(", ", rows)}; colour space={PlayerSettings.colorSpace}; renderer post data={(renderer.postProcessData != null)}");
    }

    static void Set(SerializedObject so, string field, bool value)
    {
        var p = so.FindProperty(field);
        if (p == null) throw new System.Exception($"URP asset has no field {field}; set it in the Inspector");
        p.boolValue = value;
    }

    static void Set(SerializedObject so, string field, int value)
    {
        var p = so.FindProperty(field);
        if (p == null) throw new System.Exception($"URP asset has no field {field}; set it in the Inspector");
        if (p.propertyType == SerializedPropertyType.Enum) p.enumValueIndex = value; else p.intValue = value;
    }
}
