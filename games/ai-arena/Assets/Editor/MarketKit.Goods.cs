using System.Linq;
using UnityEditor;
using UnityEngine;

// AI Arena > Rebuild market goods: the goods on the stalls only (a crate heaped with the item and
// a pile beside it, per item), from the same recipe as Build market kit, without touching the
// rest of the kit. Run after the merge fix of 8 November 2026, when the saved goods meshes were
// found empty.
public static partial class MarketKit
{
    [MenuItem("AI Arena/Rebuild market goods")]
    public static void RebuildGoods()
    {
        var crate = AssetDatabase.LoadAssetAtPath<GameObject>($"{Out}/Hero/Crate.prefab");
        if (crate == null) { Debug.LogError("[MarketKit] no crate prefab: run Build market kit"); return; }
        foreach (var id in Specs.Keys) BuildGoods(id, crate);
        AssetDatabase.SaveAssets();
        foreach (var id in Specs.Keys)
        {
            var g = AssetDatabase.LoadAssetAtPath<GameObject>($"{Out}/Goods/{id}.prefab");
            var merged = g == null ? new MeshFilter[0] : g.GetComponentsInChildren<MeshFilter>(true).Where(f => f.name == "Merged").ToArray();
            int v = merged.Sum(f => f.sharedMesh != null ? f.sharedMesh.vertexCount : 0);
            long t = merged.Sum(f => f.sharedMesh != null ? (long)f.sharedMesh.GetIndexCount(0) / 3 : 0);
            Debug.Log($"[MarketKit] goods {id}: {v} vertices, {t} triangles merged");
        }
    }

    public static void RebuildGoodsBatch() { RebuildGoods(); EditorApplication.Exit(0); }
}
