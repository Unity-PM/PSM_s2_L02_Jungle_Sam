#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.AI.Navigation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;

namespace JungleSam.EditorTools
{
    public static class JungleSamNavMeshWaterTool
    {
        private const string WaterLayerName = "Water";
        private const string UnderwaterVolumeName = "JS_Underwater_NotWalkable_Volume";
        private const float UnderwaterVolumeDepth = 35f;
        private const float UnderwaterVolumeTopClearance = 0.2f;
        private const float UnderwaterVolumeBoundsPadding = 12f;
        private const string BridgeProxyNamePrefix = "JS_NavMesh_BridgeProxy";
        private const float BridgeProxyThickness = 0.08f;
        private const float BridgeProxyHeightOffset = 0.04f;

        private static readonly string[] WaterTokens =
        {
            "3D_Water",
            "WaterPlane",
            "Water"
        };

        [MenuItem("Tools/Jungle Sam/NavMesh/Report Water NavMesh Setup")]
        public static void ReportWaterNavMeshSetup()
        {
            Debug.Log(BuildReport());
        }

        [MenuItem("Tools/Jungle Sam/NavMesh/Prepare Water For NavMesh Rebuild")]
        public static void PrepareWaterForNavMeshRebuild()
        {
            int waterLayer = LayerMask.NameToLayer(WaterLayerName);

            if (waterLayer < 0)
            {
                Debug.LogError($"[JungleSam NavMesh] Layer '{WaterLayerName}' does not exist.");
                return;
            }

            List<GameObject> waterObjects = FindWaterRendererObjects()
                .Select(renderer => renderer.gameObject)
                .Distinct()
                .ToList();

            NavMeshSurface[] surfaces = UnityEngine.Object.FindObjectsByType<NavMeshSurface>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);

            if (waterObjects.Count == 0 && surfaces.Length == 0)
            {
                Debug.LogWarning("[JungleSam NavMesh] No water renderers or NavMeshSurface components found in the active scene.");
                return;
            }

            Undo.RecordObjects(waterObjects.Cast<UnityEngine.Object>().Concat(surfaces).ToArray(), "Prepare Water For NavMesh Rebuild");

            int changedWaterObjects = 0;
            foreach (GameObject waterObject in waterObjects)
            {
                if (waterObject.layer == waterLayer)
                    continue;

                waterObject.layer = waterLayer;
                EditorUtility.SetDirty(waterObject);
                changedWaterObjects++;
            }

            int waterMask = 1 << waterLayer;
            int changedSurfaces = 0;

            foreach (NavMeshSurface surface in surfaces)
            {
                if ((surface.layerMask.value & waterMask) == 0)
                    continue;

                surface.layerMask &= ~waterMask;
                EditorUtility.SetDirty(surface);
                changedSurfaces++;
            }

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

            StringBuilder builder = new StringBuilder();
            builder.AppendLine("[JungleSam NavMesh] Water setup prepared.");
            builder.AppendLine($"Water render objects set to '{WaterLayerName}' layer: {changedWaterObjects}/{waterObjects.Count}");
            builder.AppendLine($"NavMeshSurface components excluding '{WaterLayerName}': {changedSurfaces}/{surfaces.Length}");
            builder.AppendLine($"Use '{UnderwaterVolumeName}' to block the underwater floor while keeping bridges above the water walkable.");
            builder.AppendLine("Next step: open each NavMeshSurface in the active scene and bake/rebuild NavMeshData.");
            Debug.Log(builder.ToString());
        }

        [MenuItem("Tools/Jungle Sam/NavMesh/Create Or Update Underwater Not Walkable Volume")]
        public static void CreateOrUpdateUnderwaterNotWalkableVolume()
        {
            List<Renderer> waterRenderers = FindWaterRendererObjects().ToList();

            if (waterRenderers.Count == 0)
            {
                Debug.LogWarning("[JungleSam NavMesh] No water renderers found in the active scene.");
                return;
            }

            if (!TryBuildWaterBounds(waterRenderers, out Bounds waterBounds))
            {
                Debug.LogWarning("[JungleSam NavMesh] Could not calculate water bounds.");
                return;
            }

            float waterHeight = GetMedianWaterHeight(waterRenderers);
            float volumeTop = waterHeight - UnderwaterVolumeTopClearance;
            float volumeDepth = Mathf.Max(1f, UnderwaterVolumeDepth);

            GameObject volumeObject = GameObject.Find(UnderwaterVolumeName);
            if (volumeObject == null)
            {
                volumeObject = new GameObject(UnderwaterVolumeName);
                Undo.RegisterCreatedObjectUndo(volumeObject, "Create Underwater Not Walkable Volume");
            }
            else
            {
                Undo.RecordObject(volumeObject, "Update Underwater Not Walkable Volume");
            }

            NavMeshModifierVolume modifierVolume = volumeObject.GetComponent<NavMeshModifierVolume>();
            if (modifierVolume == null)
            {
                modifierVolume = Undo.AddComponent<NavMeshModifierVolume>(volumeObject);
            }
            else
            {
                Undo.RecordObject(modifierVolume, "Update Underwater Not Walkable Volume");
            }

            int notWalkableArea = NavMesh.GetAreaFromName("Not Walkable");
            if (notWalkableArea < 0)
                notWalkableArea = 1;

            volumeObject.transform.position = Vector3.zero;
            modifierVolume.area = notWalkableArea;
            modifierVolume.center = new Vector3(
                waterBounds.center.x,
                volumeTop - volumeDepth * 0.5f,
                waterBounds.center.z);
            modifierVolume.size = new Vector3(
                waterBounds.size.x + UnderwaterVolumeBoundsPadding * 2f,
                volumeDepth,
                waterBounds.size.z + UnderwaterVolumeBoundsPadding * 2f);

            EditorUtility.SetDirty(volumeObject);
            EditorUtility.SetDirty(modifierVolume);
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

            StringBuilder builder = new StringBuilder();
            builder.AppendLine("[JungleSam NavMesh] Underwater Not Walkable volume updated.");
            builder.AppendLine($"Volume: {UnderwaterVolumeName}");
            builder.AppendLine($"Water height: {waterHeight:F2}");
            builder.AppendLine($"Volume top: {volumeTop:F2}");
            builder.AppendLine($"Volume depth: {volumeDepth:F2}");
            builder.AppendLine($"Volume size: {modifierVolume.size}");
            builder.AppendLine("Next step: bake/rebuild NavMeshData. Bridges above the volume should remain walkable.");
            Debug.Log(builder.ToString());
        }

        [MenuItem("Tools/Jungle Sam/NavMesh/Create Walkable Bridge Proxy From Selection")]
        public static void CreateWalkableBridgeProxyFromSelection()
        {
            if (!TryBuildSelectionBounds(out Bounds selectionBounds))
            {
                Debug.LogWarning("[JungleSam NavMesh] Select the bridge deck/planks first, then run this command.");
                return;
            }

            GameObject proxy = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Undo.RegisterCreatedObjectUndo(proxy, "Create Walkable Bridge Proxy");

            proxy.name = $"{BridgeProxyNamePrefix}_{Selection.activeGameObject.name}";
            proxy.tag = "EditorOnly";

            int groundLayer = LayerMask.NameToLayer("Ground");
            if (groundLayer >= 0)
                proxy.layer = groundLayer;

            float thickness = Mathf.Max(0.02f, BridgeProxyThickness);
            float topY = selectionBounds.min.y + BridgeProxyHeightOffset;
            proxy.transform.position = new Vector3(selectionBounds.center.x, topY - thickness * 0.5f, selectionBounds.center.z);
            proxy.transform.rotation = Quaternion.identity;
            proxy.transform.localScale = new Vector3(
                Mathf.Max(0.5f, selectionBounds.size.x),
                thickness,
                Mathf.Max(0.5f, selectionBounds.size.z));

            Collider proxyCollider = proxy.GetComponent<Collider>();
            if (proxyCollider != null)
                UnityEngine.Object.DestroyImmediate(proxyCollider);

            Renderer proxyRenderer = proxy.GetComponent<Renderer>();
            if (proxyRenderer != null)
            {
                proxyRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                proxyRenderer.receiveShadows = false;
            }

            EditorUtility.SetDirty(proxy);
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

            StringBuilder builder = new StringBuilder();
            builder.AppendLine("[JungleSam NavMesh] Walkable bridge proxy created.");
            builder.AppendLine($"Proxy: {proxy.name}");
            builder.AppendLine($"Layer: {LayerMask.LayerToName(proxy.layer)}");
            builder.AppendLine($"Tag: {proxy.tag}");
            builder.AppendLine($"Position: {proxy.transform.position}");
            builder.AppendLine($"Scale: {proxy.transform.localScale}");
            builder.AppendLine("Move/scale it manually if needed, then bake/rebuild NavMeshData.");
            Debug.Log(builder.ToString());
            Selection.activeGameObject = proxy;
        }

        private static string BuildReport()
        {
            int waterLayer = LayerMask.NameToLayer(WaterLayerName);
            List<Renderer> waterRenderers = FindWaterRendererObjects().ToList();
            NavMeshSurface[] surfaces = UnityEngine.Object.FindObjectsByType<NavMeshSurface>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);

            int waterObjectsOnWaterLayer = waterRenderers.Count(renderer => renderer.gameObject.layer == waterLayer);
            int surfacesIncludingWaterLayer = waterLayer < 0
                ? 0
                : surfaces.Count(surface => (surface.layerMask.value & (1 << waterLayer)) != 0);
            NavMeshModifierVolume underwaterVolume = FindUnderwaterVolume();

            StringBuilder builder = new StringBuilder();
            builder.AppendLine("[JungleSam NavMesh] Water setup report");
            builder.AppendLine($"Scene: {EditorSceneManager.GetActiveScene().path}");
            builder.AppendLine($"Water layer index: {waterLayer}");
            builder.AppendLine($"Detected water renderers: {waterRenderers.Count}");
            builder.AppendLine($"Water renderers on Water layer: {waterObjectsOnWaterLayer}/{waterRenderers.Count}");
            builder.AppendLine($"NavMeshSurface components: {surfaces.Length}");
            builder.AppendLine($"NavMeshSurface components still including Water layer: {surfacesIncludingWaterLayer}");
            builder.AppendLine($"Underwater Not Walkable volume: {(underwaterVolume != null ? "present" : "missing")}");
            if (underwaterVolume != null)
            {
                builder.AppendLine($"  Center: {underwaterVolume.center}");
                builder.AppendLine($"  Size: {underwaterVolume.size}");
                builder.AppendLine($"  Area: {underwaterVolume.area}");
            }
            builder.AppendLine();
            builder.AppendLine("Sample water objects:");

            foreach (Renderer renderer in waterRenderers.Take(12))
                builder.AppendLine($"- {GetHierarchyPath(renderer.transform)} | layer {LayerMask.LayerToName(renderer.gameObject.layer)}");

            return builder.ToString();
        }

        private static IEnumerable<Renderer> FindWaterRendererObjects()
        {
            return UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                .Where(renderer => renderer.gameObject.scene.IsValid())
                .Where(IsWaterRenderer);
        }

        private static NavMeshModifierVolume FindUnderwaterVolume()
        {
            GameObject volumeObject = GameObject.Find(UnderwaterVolumeName);
            return volumeObject != null ? volumeObject.GetComponent<NavMeshModifierVolume>() : null;
        }

        private static bool TryBuildWaterBounds(IReadOnlyList<Renderer> waterRenderers, out Bounds waterBounds)
        {
            waterBounds = default;

            if (waterRenderers.Count == 0)
                return false;

            bool initialized = false;

            foreach (Renderer waterRenderer in waterRenderers)
            {
                if (waterRenderer == null)
                    continue;

                if (!initialized)
                {
                    waterBounds = waterRenderer.bounds;
                    initialized = true;
                    continue;
                }

                waterBounds.Encapsulate(waterRenderer.bounds);
            }

            return initialized;
        }

        private static bool TryBuildSelectionBounds(out Bounds selectionBounds)
        {
            selectionBounds = default;
            GameObject[] selectedObjects = Selection.gameObjects;

            if (selectedObjects == null || selectedObjects.Length == 0)
                return false;

            bool initialized = false;

            foreach (GameObject selectedObject in selectedObjects)
            {
                if (selectedObject == null)
                    continue;

                Renderer[] renderers = selectedObject.GetComponentsInChildren<Renderer>(true);

                foreach (Renderer renderer in renderers)
                {
                    if (renderer == null)
                        continue;

                    if (!initialized)
                    {
                        selectionBounds = renderer.bounds;
                        initialized = true;
                        continue;
                    }

                    selectionBounds.Encapsulate(renderer.bounds);
                }

                if (renderers.Length > 0)
                    continue;

                Collider[] colliders = selectedObject.GetComponentsInChildren<Collider>(true);

                foreach (Collider collider in colliders)
                {
                    if (collider == null)
                        continue;

                    if (!initialized)
                    {
                        selectionBounds = collider.bounds;
                        initialized = true;
                        continue;
                    }

                    selectionBounds.Encapsulate(collider.bounds);
                }
            }

            return initialized;
        }

        private static float GetMedianWaterHeight(IReadOnlyList<Renderer> waterRenderers)
        {
            List<float> heights = waterRenderers
                .Where(renderer => renderer != null)
                .Select(renderer => renderer.transform.position.y)
                .OrderBy(height => height)
                .ToList();

            if (heights.Count == 0)
                return 0f;

            return heights[heights.Count / 2];
        }

        private static bool IsWaterRenderer(Renderer renderer)
        {
            string hierarchyPath = GetHierarchyPath(renderer.transform);

            if (ContainsAny(hierarchyPath, WaterTokens))
                return true;

            foreach (Material material in renderer.sharedMaterials)
            {
                if (material != null && ContainsAny(material.name, WaterTokens))
                    return true;
            }

            return false;
        }

        private static string GetHierarchyPath(Transform transform)
        {
            Stack<string> names = new Stack<string>();
            Transform current = transform;

            while (current != null)
            {
                names.Push(current.name);
                current = current.parent;
            }

            return string.Join("/", names);
        }

        private static bool ContainsAny(string value, IEnumerable<string> tokens)
        {
            return tokens.Any(token => value.IndexOf(token, StringComparison.OrdinalIgnoreCase) >= 0);
        }
    }
}
#endif
