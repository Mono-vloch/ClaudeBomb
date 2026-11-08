using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

// AI Arena > Build family: Mom (Higgsfield, Meshy 7, rigged with a one-hand wave) as the prefab
// Resources/Market/Family, stood at the family's door by Market.FamilySpot. The same cleanup as
// the other heroes: 1.65 m tall, pivot at her feet, facing +Z, one URP Lit material from her maps
// (Models/Hero/Mom, split by glb_maps.py), lit by the scene's light. AI Arena > Preview family
// renders her at rest and through the wave into Builds/look-audit/game-feel/mom.
public static partial class MarketKit
{
    const float MomHeight = 1.65f;

    [MenuItem("AI Arena/Build family")]
    public static void BuildFamily()
    {
        string glb = $"{HeroDir}/Mom/mom.glb";
        var source = AssetDatabase.LoadAssetAtPath<GameObject>(glb);
        if (source == null) { Debug.LogError("[MarketKit] family: " + glb + " missing"); return; }
        var clip = AssetDatabase.LoadAllAssetsAtPath(glb).OfType<AnimationClip>().FirstOrDefault(c => !c.name.StartsWith("__preview"));
        var mat = HeroMaterial("Mom", "mom", 1024, detailSize: 1024);
        var root = new GameObject("Family");
        var model = Object.Instantiate(source);
        model.name = "Mom";
        model.transform.SetParent(root.transform, false);
        foreach (var r in model.GetComponentsInChildren<Renderer>()) { r.sharedMaterials = r.sharedMaterials.Select(_ => mat).ToArray(); r.shadowCastingMode = ShadowCastingMode.On; }
        foreach (var s in model.GetComponentsInChildren<SkinnedMeshRenderer>()) s.updateWhenOffscreen = false;
        foreach (var a in model.GetComponentsInChildren<Animation>()) Object.DestroyImmediate(a);
        var b = Bounds(model);
        if (b.size.y > 0.01f) model.transform.localScale *= MomHeight / b.size.y;
        b = Bounds(model);
        model.transform.localPosition -= new Vector3(b.center.x, b.min.y, b.center.z);
        var animator = model.GetComponent<Animator>() ?? model.AddComponent<Animator>();
        animator.applyRootMotion = false;
        animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
        if (clip != null)
        {
            var copy = Object.Instantiate(clip);
            copy.name = "mom_wave";
            var settings = AnimationUtility.GetAnimationClipSettings(copy);
            settings.loopTime = false;
            AnimationUtility.SetAnimationClipSettings(copy, settings);
            string clipPath = $"{Generated}/Clips/mom_wave.anim";
            AssetDatabase.DeleteAsset(clipPath);
            AssetDatabase.CreateAsset(copy, clipPath);
            string controllerPath = $"{Generated}/mom.controller";
            AssetDatabase.DeleteAsset(controllerPath);
            animator.runtimeAnimatorController = AnimatorController.CreateAnimatorControllerAtPathWithClip(controllerPath, copy);
        }
        var rig = root.AddComponent<MomRig>();
        rig.animator = animator;
        rig.waveSeconds = clip != null ? clip.length : 4f;
        b = Bounds(root);
        int tris = model.GetComponentsInChildren<SkinnedMeshRenderer>(true).Sum(s => s.sharedMesh != null ? s.sharedMesh.triangles.Length / 3 : 0);
        PrefabUtility.SaveAsPrefabAsset(root, $"{Out}/Family.prefab");
        Object.DestroyImmediate(root);
        AssetDatabase.SaveAssets();
        Debug.Log($"[MarketKit] family: {b.size.x:F2} x {b.size.y:F2} x {b.size.z:F2} m, clip {(clip != null ? clip.name + " " + clip.length.ToString("F2") + " s" : "MISSING")}, {tris} triangles");
    }

    [MenuItem("AI Arena/Preview family")]
    public static void PreviewFamily()
    {
        string dir = "Builds/look-audit/game-feel/mom";
        Directory.CreateDirectory(dir);
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{Out}/Family.prefab");
        var mom = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        var model = mom.transform.Find("Mom").gameObject;
        foreach (var s in mom.GetComponentsInChildren<SkinnedMeshRenderer>()) s.forceMatrixRecalculationPerRender = true;
        var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>($"{Generated}/Clips/mom_wave.anim");
        var sun = new GameObject("Sun").AddComponent<Light>();
        sun.type = LightType.Directional; sun.intensity = 1.3f; sun.transform.rotation = Quaternion.Euler(35f, 160f, 0f);
        RenderSettings.ambientMode = AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = new Color(0.55f, 0.6f, 0.7f); RenderSettings.ambientEquatorColor = new Color(0.45f, 0.42f, 0.4f); RenderSettings.ambientGroundColor = new Color(0.2f, 0.2f, 0.22f);
        var cam = new GameObject("Camera").AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor; cam.backgroundColor = new Color(0.82f, 0.84f, 0.86f);
        cam.fieldOfView = 30f;
        void Shot(string name, Vector3 at, Vector3 look, float t)
        {
            if (clip != null) clip.SampleAnimation(model, t);
            cam.transform.position = at; cam.transform.LookAt(look);
            var rt = new RenderTexture(800, 1000, 24);
            cam.targetTexture = rt; cam.Render();
            RenderTexture.active = rt;
            var tex = new Texture2D(800, 1000, TextureFormat.RGB24, false); tex.ReadPixels(new Rect(0, 0, 800, 1000), 0, 0); tex.Apply();
            File.WriteAllBytes($"{dir}/{name}.png", tex.EncodeToPNG());
            cam.targetTexture = null; RenderTexture.active = null; Object.DestroyImmediate(tex); rt.Release();
        }
        float len = clip != null ? clip.length : 1f;
        Shot("1-front-rest", new Vector3(0f, 0.95f, 4.2f), new Vector3(0f, 0.85f, 0f), 0f);
        Shot("2-face", new Vector3(0f, 1.5f, 1.0f), new Vector3(0f, 1.45f, 0f), 0f);
        Shot("3-hands-rest", new Vector3(0.5f, 0.9f, 1.6f), new Vector3(0f, 0.85f, 0f), 0f);
        for (int i = 1; i <= 4; i++) Shot($"4-wave-{i}", new Vector3(0f, 0.95f, 4.2f), new Vector3(0f, 0.85f, 0f), len * i / 4.5f);
        Shot("5-wave-hand", new Vector3(0.4f, 1.5f, 1.4f), new Vector3(0.2f, 1.35f, 0f), len * 0.45f);
        Shot("6-side", new Vector3(4.2f, 0.95f, 0f), new Vector3(0f, 0.85f, 0f), 0f);
        Shot("7-end", new Vector3(0f, 0.95f, 4.2f), new Vector3(0f, 0.85f, 0f), len);
        Debug.Log("[MarketKit] family previews in " + dir);
    }

    public static void BuildFamilyBatch() { BuildFamily(); PreviewFamily(); EditorApplication.Exit(0); }
}
