#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace JungleSam.EditorTools
{
    public sealed class JungleSamRenderingOptimizationTool : EditorWindow
    {
        private static readonly string[] VegetationNameTokens =
        {
            "Grass",
            "Bush",
            "Tree",
            "TreeCreator",
            "Leaf",
            "Leaves",
            "Branch",
            "Trunk",
            "Plant",
            "Reed",
            "Fern",
            "Weed",
            "Flower",
            "Nature",
            "Vegetation",
            "3D_Water",
            "Water"
        };

        private static readonly string[] VegetationAssetPathTokens =
        {
            "Flooded_Grounds/Content/Trees",
            "Flooded_Grounds/Content/Nature",
            "Flooded_Grounds/Content/Grass",
            "Flooded_Grounds/Content/Plants"
        };

        private static readonly string[] ProtectedNameTokens =
        {
            "Player",
            "Weapon",
            "Enemy",
            "Zombie",
            "Monster",
            "Mutant",
            "Building",
            "House",
            "Bridge",
            "Boat",
            "Rock",
            "Church",
            "Road",
            "Ground",
            "Terrain",
            "Wall"
        };

        private static readonly string[] EnemyPrefabFolders =
        {
            "Assets/JungleSam/Prefabs/Enemies",
            "Assets/ThirdParty/Models"
        };

        private const string ShadowBackupPath = "Library/JungleSamOptimizationShadowBackup.json";

        private Vector2 scrollPosition;
        private string reportText = "Choose a report or optimization action.";

        [MenuItem("Tools/Jungle Sam/Optimization/Rendering Optimization Tool")]
        public static void OpenWindow()
        {
            GetWindow<JungleSamRenderingOptimizationTool>("Jungle Rendering Optimization");
        }

        [MenuItem("Tools/Jungle Sam/Optimization/Report Active Scene Rendering")]
        public static void MenuReportActiveScene()
        {
            string report = BuildActiveSceneRenderingReport();
            Debug.Log(report);
        }

        [MenuItem("Tools/Jungle Sam/Optimization/Apply Vegetation Shadow Optimization")]
        public static void MenuApplyVegetationShadowOptimization()
        {
            ApplyVegetationShadowOptimizationWithConfirmation();
        }

        [MenuItem("Tools/Jungle Sam/Optimization/Optimize All Flooded Grounds Vegetation Shadows")]
        public static void MenuOptimizeAllFloodedGroundsVegetationShadows()
        {
            OptimizeAllFloodedGroundsVegetationShadowsWithConfirmation();
        }

        [MenuItem("Tools/Jungle Sam/Optimization/Report Enemy Prefabs")]
        public static void MenuReportEnemyPrefabs()
        {
            string report = BuildEnemyPrefabReport();
            Debug.Log(report);
        }

        [MenuItem("Tools/Jungle Sam/Optimization/Restore Vegetation Cast Shadows ON")]
        public static void MenuRestoreVegetationCastShadows()
        {
            RestoreVegetationCastShadowsWithConfirmation();
        }

        [MenuItem("Tools/Jungle Sam/Optimization/Restore Last Shadow Backup")]
        public static void MenuRestoreLastShadowBackup()
        {
            RestoreLastShadowBackupWithConfirmation();
        }

        [MenuItem("Tools/Jungle Sam/Optimization/Emergency Restore All Scene Cast Shadows ON")]
        public static void MenuEmergencyRestoreAllSceneCastShadows()
        {
            EmergencyRestoreAllSceneCastShadowsWithConfirmation();
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Jungle Sam Rendering Optimization", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Scene optimization is limited to renderer shadow flags. It does not touch gameplay scripts, animation clips, AI, colliders, damage, input, or spawn logic.",
                MessageType.Info);

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Report Active Scene", GUILayout.Height(32f)))
                {
                    reportText = BuildActiveSceneRenderingReport();
                    Debug.Log(reportText);
                }

                if (GUILayout.Button("Optimize Vegetation Shadows", GUILayout.Height(32f)))
                {
                    reportText = ApplyVegetationShadowOptimizationWithConfirmation();
                }

                if (GUILayout.Button("Report Enemy Prefabs", GUILayout.Height(32f)))
                {
                    reportText = BuildEnemyPrefabReport();
                    Debug.Log(reportText);
                }
            }

            if (GUILayout.Button("Optimize All Flooded Grounds Vegetation Shadows", GUILayout.Height(30f)))
            {
                reportText = OptimizeAllFloodedGroundsVegetationShadowsWithConfirmation();
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Restore Last Backup", GUILayout.Height(28f)))
                {
                    reportText = RestoreLastShadowBackupWithConfirmation();
                }

                if (GUILayout.Button("Restore Vegetation Shadows ON", GUILayout.Height(28f)))
                {
                    reportText = RestoreVegetationCastShadowsWithConfirmation();
                }

                if (GUILayout.Button("Emergency Restore All Shadows ON", GUILayout.Height(28f)))
                {
                    reportText = EmergencyRestoreAllSceneCastShadowsWithConfirmation();
                }
            }

            EditorGUILayout.Space(8f);
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            EditorGUILayout.TextArea(reportText, GUILayout.ExpandHeight(true));
            EditorGUILayout.EndScrollView();
        }

        private static string ApplyVegetationShadowOptimizationWithConfirmation()
        {
            return ApplyVegetationShadowOptimizationWithConfirmation("Jungle Sam Vegetation Shadow Optimization");
        }

        private static string OptimizeAllFloodedGroundsVegetationShadowsWithConfirmation()
        {
            return ApplyVegetationShadowOptimizationWithConfirmation("Optimize All Flooded Grounds Vegetation Shadows");
        }

        private static string ApplyVegetationShadowOptimizationWithConfirmation(string dialogTitle)
        {
            List<Renderer> candidates = FindVegetationRenderers()
                .Where(renderer => renderer.shadowCastingMode != ShadowCastingMode.Off || !renderer.receiveShadows)
                .ToList();

            if (candidates.Count == 0)
            {
                string noChanges = "[JungleSam Optimization] No vegetation renderers need shadow changes.";
                Debug.Log(noChanges);
                return noChanges;
            }

            bool apply = EditorUtility.DisplayDialog(
                dialogTitle,
                $"This will update {candidates.Count} vegetation renderers in the active scene:\n\nCast Shadows: Off\nReceive Shadows: On\n\nProtected names are skipped.",
                "Apply",
                "Cancel");

            if (!apply)
            {
                string cancelled = "[JungleSam Optimization] Vegetation shadow optimization cancelled.";
                Debug.Log(cancelled);
                return cancelled;
            }

            SaveShadowBackup(candidates);
            Undo.RecordObjects(candidates.Cast<UnityEngine.Object>().ToArray(), "Optimize Vegetation Shadows");

            foreach (Renderer renderer in candidates)
            {
                renderer.shadowCastingMode = ShadowCastingMode.Off;
                renderer.receiveShadows = true;
                EditorUtility.SetDirty(renderer);
            }

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

            string result = BuildVegetationOptimizationSummary(candidates);
            Debug.Log(result);
            return result;
        }

        private static string RestoreVegetationCastShadowsWithConfirmation()
        {
            List<Renderer> candidates = FindVegetationRenderers()
                .Where(renderer => renderer.shadowCastingMode == ShadowCastingMode.Off)
                .ToList();

            if (candidates.Count == 0)
            {
                string noChanges = "[JungleSam Optimization] No vegetation renderers have Cast Shadows OFF.";
                Debug.Log(noChanges);
                return noChanges;
            }

            bool restore = EditorUtility.DisplayDialog(
                "Restore Vegetation Cast Shadows",
                $"This will set Cast Shadows ON for {candidates.Count} vegetation renderers in the active scene.\n\nReceive Shadows will not be changed.",
                "Restore",
                "Cancel");

            if (!restore)
            {
                string cancelled = "[JungleSam Optimization] Vegetation shadow restore cancelled.";
                Debug.Log(cancelled);
                return cancelled;
            }

            Undo.RecordObjects(candidates.Cast<UnityEngine.Object>().ToArray(), "Restore Vegetation Cast Shadows");

            foreach (Renderer renderer in candidates)
            {
                renderer.shadowCastingMode = ShadowCastingMode.On;
                EditorUtility.SetDirty(renderer);
            }

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

            string result = BuildShadowRestoreSummary("Vegetation Cast Shadows restored to ON", candidates);
            Debug.Log(result);
            return result;
        }

        private static string EmergencyRestoreAllSceneCastShadowsWithConfirmation()
        {
            List<Renderer> candidates = FindActiveSceneRenderers()
                .Where(renderer => renderer.shadowCastingMode == ShadowCastingMode.Off)
                .Where(renderer => !(renderer is ParticleSystemRenderer))
                .ToList();

            if (candidates.Count == 0)
            {
                string noChanges = "[JungleSam Optimization] No scene renderers have Cast Shadows OFF.";
                Debug.Log(noChanges);
                return noChanges;
            }

            bool restore = EditorUtility.DisplayDialog(
                "Emergency Restore All Scene Cast Shadows",
                $"This will set Cast Shadows ON for {candidates.Count} non-particle renderers in the active scene.\n\nUse this only if a previous action disabled shadows too broadly.",
                "Restore All",
                "Cancel");

            if (!restore)
            {
                string cancelled = "[JungleSam Optimization] Emergency restore cancelled.";
                Debug.Log(cancelled);
                return cancelled;
            }

            Undo.RecordObjects(candidates.Cast<UnityEngine.Object>().ToArray(), "Emergency Restore All Scene Cast Shadows");

            foreach (Renderer renderer in candidates)
            {
                renderer.shadowCastingMode = ShadowCastingMode.On;
                EditorUtility.SetDirty(renderer);
            }

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

            string result = BuildShadowRestoreSummary("Emergency scene Cast Shadows restored to ON", candidates);
            Debug.Log(result);
            return result;
        }

        private static string RestoreLastShadowBackupWithConfirmation()
        {
            if (!File.Exists(ShadowBackupPath))
            {
                string missing = $"[JungleSam Optimization] No shadow backup found at {ShadowBackupPath}.";
                Debug.LogWarning(missing);
                return missing;
            }

            ShadowBackup backup = JsonUtility.FromJson<ShadowBackup>(File.ReadAllText(ShadowBackupPath));
            if (backup == null || backup.renderers == null || backup.renderers.Count == 0)
            {
                string empty = "[JungleSam Optimization] Shadow backup is empty.";
                Debug.LogWarning(empty);
                return empty;
            }

            List<Renderer> renderers = ResolveBackupRenderers(backup);
            if (renderers.Count == 0)
            {
                string unresolved = "[JungleSam Optimization] Shadow backup could not resolve any renderers in the active project.";
                Debug.LogWarning(unresolved);
                return unresolved;
            }

            bool restore = EditorUtility.DisplayDialog(
                "Restore Last Shadow Backup",
                $"This will restore Cast Shadows and Receive Shadows for {renderers.Count} renderers from the last optimization backup.",
                "Restore Backup",
                "Cancel");

            if (!restore)
            {
                string cancelled = "[JungleSam Optimization] Shadow backup restore cancelled.";
                Debug.Log(cancelled);
                return cancelled;
            }

            Undo.RecordObjects(renderers.Cast<UnityEngine.Object>().ToArray(), "Restore Shadow Backup");

            int restored = 0;
            foreach (ShadowBackupEntry entry in backup.renderers)
            {
                Renderer renderer = ResolveRenderer(entry.globalObjectId);
                if (renderer == null)
                {
                    continue;
                }

                renderer.shadowCastingMode = (ShadowCastingMode)entry.shadowCastingMode;
                renderer.receiveShadows = entry.receiveShadows;
                EditorUtility.SetDirty(renderer);
                restored++;
            }

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

            string result = $"[JungleSam Optimization] Restored shadow backup for {restored} renderers.";
            Debug.Log(result);
            return result;
        }

        private static string BuildActiveSceneRenderingReport()
        {
            Renderer[] renderers = FindActiveSceneRenderers();
            SkinnedMeshRenderer[] skinnedMeshRenderers = renderers.OfType<SkinnedMeshRenderer>().ToArray();
            int shadowCasters = renderers.Count(renderer => renderer.shadowCastingMode != ShadowCastingMode.Off);
            int vegetationCandidates = FindVegetationRenderers().Count;

            StringBuilder builder = new StringBuilder();
            builder.AppendLine("[JungleSam Optimization] Active scene rendering report");
            builder.AppendLine($"Scene: {EditorSceneManager.GetActiveScene().path}");
            builder.AppendLine($"Renderers: {renderers.Length}");
            builder.AppendLine($"SkinnedMeshRenderers: {skinnedMeshRenderers.Length}");
            builder.AppendLine($"Cast Shadows ON: {shadowCasters}");
            builder.AppendLine($"Vegetation candidates: {vegetationCandidates}");
            builder.AppendLine();
            builder.AppendLine("Top 20 hierarchy/prefab groups by renderer count:");

            foreach (RendererGroup group in BuildRendererGroups(renderers).Take(20))
            {
                builder.AppendLine($"- {group.RendererCount,4} renderers | {group.Name} | {group.AssetPath}");
            }

            return builder.ToString();
        }

        private static string BuildEnemyPrefabReport()
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("[JungleSam Optimization] Enemy prefab rendering report");

            string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", EnemyPrefabFolders)
                .Distinct()
                .ToArray();

            List<EnemyPrefabStats> stats = prefabGuids
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(IsEnemyPrefabPath)
                .Select(BuildEnemyPrefabStats)
                .Where(stat => stat != null)
                .OrderByDescending(stat => stat.SkinnedMeshRendererCount)
                .ThenBy(stat => stat.Path, StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (stats.Count == 0)
            {
                builder.AppendLine("No enemy prefabs found.");
                return builder.ToString();
            }

            foreach (EnemyPrefabStats stat in stats)
            {
                builder.AppendLine();
                builder.AppendLine(stat.Path);
                builder.AppendLine($"  Renderers: {stat.RendererCount}");
                builder.AppendLine($"  SkinnedMeshRenderers: {stat.SkinnedMeshRendererCount}");
                builder.AppendLine($"  Cast Shadows ON: {stat.ShadowCasterCount}");
                builder.AppendLine($"  Small shadow casters: {stat.SmallShadowCasterNames.Count}");
                builder.AppendLine($"  Skinned Update When Offscreen ON: {stat.UpdateWhenOffscreenCount}");
                builder.AppendLine($"  Animator Culling Modes: {string.Join(", ", stat.AnimatorCullingModes)}");

                if (stat.SmallShadowCasterNames.Count > 0)
                {
                    builder.AppendLine($"  Small shadow caster samples: {string.Join(", ", stat.SmallShadowCasterNames.Take(8))}");
                }
            }

            builder.AppendLine();
            builder.AppendLine("Notes:");
            builder.AppendLine("- Enemy prefabs were reported only. No enemy prefab values were changed.");
            builder.AppendLine("- Update When Offscreen should usually stay OFF for skinned renderers unless a specific mesh visually breaks.");
            builder.AppendLine("- Animator culling affects animation evaluation and should be changed only after AI/hitbox testing.");

            return builder.ToString();
        }

        private static EnemyPrefabStats BuildEnemyPrefabStats(string path)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null)
            {
                return null;
            }

            Renderer[] renderers = prefab.GetComponentsInChildren<Renderer>(true);
            SkinnedMeshRenderer[] skinnedMeshRenderers = prefab.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            Animator[] animators = prefab.GetComponentsInChildren<Animator>(true);

            List<string> smallShadowCasters = renderers
                .Where(renderer => renderer.shadowCastingMode != ShadowCastingMode.Off)
                .Where(IsSmallRenderer)
                .Select(renderer => renderer.name)
                .Distinct()
                .Take(12)
                .ToList();

            return new EnemyPrefabStats
            {
                Path = path,
                RendererCount = renderers.Length,
                SkinnedMeshRendererCount = skinnedMeshRenderers.Length,
                ShadowCasterCount = renderers.Count(renderer => renderer.shadowCastingMode != ShadowCastingMode.Off),
                UpdateWhenOffscreenCount = skinnedMeshRenderers.Count(renderer => renderer.updateWhenOffscreen),
                SmallShadowCasterNames = smallShadowCasters,
                AnimatorCullingModes = animators
                    .Select(animator => animator.cullingMode.ToString())
                    .Distinct()
                    .DefaultIfEmpty("None")
                    .ToList()
            };
        }

        private static string BuildVegetationOptimizationSummary(IReadOnlyCollection<Renderer> changedRenderers)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("[JungleSam Optimization] Vegetation shadow optimization applied");
            builder.AppendLine($"Changed renderers: {changedRenderers.Count}");
            builder.AppendLine("Applied values: Cast Shadows OFF, Receive Shadows ON");
            builder.AppendLine($"Cast Shadows ON after optimization: {CountSceneShadowCasters()}");
            builder.AppendLine();
            builder.AppendLine("Sample changed renderers:");

            foreach (Renderer renderer in changedRenderers.Take(20))
            {
                builder.AppendLine($"- {GetHierarchyPath(renderer.transform)}");
            }

            builder.AppendLine();
            AppendTopShadowCasters(builder, FindActiveSceneRenderers(), 30);

            return builder.ToString();
        }

        private static string BuildShadowRestoreSummary(string title, IReadOnlyCollection<Renderer> changedRenderers)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine($"[JungleSam Optimization] {title}");
            builder.AppendLine($"Changed renderers: {changedRenderers.Count}");
            builder.AppendLine();
            builder.AppendLine("Sample changed renderers:");

            foreach (Renderer renderer in changedRenderers.Take(20))
            {
                builder.AppendLine($"- {GetHierarchyPath(renderer.transform)}");
            }

            return builder.ToString();
        }

        private static void SaveShadowBackup(IReadOnlyCollection<Renderer> renderers)
        {
            ShadowBackup backup = new ShadowBackup
            {
                createdUtc = DateTime.UtcNow.ToString("O"),
                scenePath = EditorSceneManager.GetActiveScene().path,
                renderers = renderers
                    .Select(renderer => new ShadowBackupEntry
                    {
                        globalObjectId = GlobalObjectId.GetGlobalObjectIdSlow(renderer).ToString(),
                        shadowCastingMode = (int)renderer.shadowCastingMode,
                        receiveShadows = renderer.receiveShadows
                    })
                    .ToList()
            };

            File.WriteAllText(ShadowBackupPath, JsonUtility.ToJson(backup, true));
        }

        private static List<Renderer> ResolveBackupRenderers(ShadowBackup backup)
        {
            return backup.renderers
                .Select(entry => ResolveRenderer(entry.globalObjectId))
                .Where(renderer => renderer != null)
                .ToList();
        }

        private static Renderer ResolveRenderer(string globalObjectId)
        {
            if (!GlobalObjectId.TryParse(globalObjectId, out GlobalObjectId parsedId))
            {
                return null;
            }

            return GlobalObjectId.GlobalObjectIdentifierToObjectSlow(parsedId) as Renderer;
        }

        private static List<RendererGroup> BuildRendererGroups(Renderer[] renderers)
        {
            Dictionary<GameObject, RendererGroup> groups = new Dictionary<GameObject, RendererGroup>();

            foreach (Renderer renderer in renderers)
            {
                GameObject groupRoot = GetRendererGroupRoot(renderer.gameObject);
                if (!groups.TryGetValue(groupRoot, out RendererGroup group))
                {
                    group = new RendererGroup
                    {
                        Name = GetHierarchyPath(groupRoot.transform),
                        AssetPath = GetPrefabAssetPath(groupRoot)
                    };
                    groups.Add(groupRoot, group);
                }

                group.RendererCount++;
            }

            return groups.Values
                .OrderByDescending(group => group.RendererCount)
                .ThenBy(group => group.Name, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private static Renderer[] FindActiveSceneRenderers()
        {
            return UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                .Where(renderer => renderer.gameObject.scene.IsValid())
                .ToArray();
        }

        private static List<Renderer> FindVegetationRenderers()
        {
            return FindActiveSceneRenderers()
                .Where(IsVegetationCandidate)
                .ToList();
        }

        private static bool IsVegetationCandidate(Renderer renderer)
        {
            RendererSearchInfo info = BuildRendererSearchInfo(renderer);

            bool hasVegetationToken =
                ContainsAny(info.HierarchyPath, VegetationNameTokens)
                || ContainsAny(info.MaterialNames, VegetationNameTokens)
                || ContainsAny(info.AssetPaths, VegetationNameTokens)
                || ContainsAny(info.AssetPaths, VegetationAssetPathTokens);

            bool isProtected =
                ContainsAny(info.HierarchyPath, ProtectedNameTokens)
                || ContainsProtectedAssetPathToken(info.AssetPaths);

            return hasVegetationToken && !isProtected;
        }

        private static bool IsEnemyPrefabPath(string path)
        {
            string normalized = path.Replace('\\', '/');
            return normalized.IndexOf("/Enemies/", StringComparison.OrdinalIgnoreCase) >= 0
                || normalized.IndexOf("Zombie", StringComparison.OrdinalIgnoreCase) >= 0
                || normalized.IndexOf("Monster", StringComparison.OrdinalIgnoreCase) >= 0
                || normalized.IndexOf("Mutant", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static bool IsSmallRenderer(Renderer renderer)
        {
            Bounds bounds = renderer is SkinnedMeshRenderer skinnedMeshRenderer
                ? skinnedMeshRenderer.localBounds
                : renderer.bounds;

            Vector3 size = bounds.size;
            float longestAxis = Mathf.Max(size.x, Mathf.Max(size.y, size.z));
            return longestAxis > 0f && longestAxis <= 0.45f;
        }

        private static GameObject GetRendererGroupRoot(GameObject gameObject)
        {
            GameObject prefabRoot = PrefabUtility.GetOutermostPrefabInstanceRoot(gameObject);
            if (prefabRoot != null)
            {
                return prefabRoot;
            }

            return gameObject.transform.root.gameObject;
        }

        private static string GetPrefabAssetPath(GameObject gameObject)
        {
            GameObject prefabRoot = PrefabUtility.GetOutermostPrefabInstanceRoot(gameObject) ?? gameObject;
            string path = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(prefabRoot);
            return string.IsNullOrEmpty(path) ? "Scene object" : path;
        }

        private static RendererSearchInfo BuildRendererSearchInfo(Renderer renderer)
        {
            RendererSearchInfo info = new RendererSearchInfo
            {
                HierarchyPath = GetHierarchyPath(renderer.transform),
                MaterialNames = string.Empty,
                AssetPaths = string.Empty
            };

            StringBuilder materialNames = new StringBuilder();
            StringBuilder assetPaths = new StringBuilder();

            AppendAssetPath(assetPaths, GetPrefabAssetPath(renderer.gameObject));

            Mesh mesh = GetRendererMesh(renderer);
            if (mesh != null)
            {
                AppendAssetPath(assetPaths, AssetDatabase.GetAssetPath(mesh));
            }

            foreach (Material material in renderer.sharedMaterials)
            {
                if (material == null)
                {
                    continue;
                }

                materialNames.Append(material.name);
                materialNames.Append(' ');
                AppendAssetPath(assetPaths, AssetDatabase.GetAssetPath(material));
            }

            info.MaterialNames = materialNames.ToString();
            info.AssetPaths = assetPaths.ToString();
            return info;
        }

        private static Mesh GetRendererMesh(Renderer renderer)
        {
            if (renderer is SkinnedMeshRenderer skinnedMeshRenderer)
            {
                return skinnedMeshRenderer.sharedMesh;
            }

            MeshFilter meshFilter = renderer.GetComponent<MeshFilter>();
            return meshFilter != null ? meshFilter.sharedMesh : null;
        }

        private static void AppendAssetPath(StringBuilder builder, string assetPath)
        {
            if (string.IsNullOrWhiteSpace(assetPath) || assetPath == "Scene object")
            {
                return;
            }

            builder.Append(assetPath.Replace('\\', '/'));
            builder.Append(' ');
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

        private static bool ContainsProtectedAssetPathToken(string assetPaths)
        {
            string sanitizedAssetPaths = assetPaths.Replace("Flooded_Grounds", "FloodedPackage");
            return ContainsAny(sanitizedAssetPaths, ProtectedNameTokens);
        }

        private static int CountSceneShadowCasters()
        {
            return FindActiveSceneRenderers()
                .Count(renderer => renderer.shadowCastingMode != ShadowCastingMode.Off);
        }

        private static void AppendTopShadowCasters(StringBuilder builder, Renderer[] renderers, int count)
        {
            builder.AppendLine($"Top {count} hierarchy/prefab groups with Cast Shadows ON:");

            Renderer[] shadowRenderers = renderers
                .Where(renderer => renderer.shadowCastingMode != ShadowCastingMode.Off)
                .ToArray();

            foreach (RendererGroup group in BuildRendererGroups(shadowRenderers).Take(count))
            {
                builder.AppendLine($"- {group.RendererCount,4} shadow casters | {group.Name} | {group.AssetPath}");
            }
        }

        private sealed class RendererGroup
        {
            public string Name;
            public string AssetPath;
            public int RendererCount;
        }

        private sealed class EnemyPrefabStats
        {
            public string Path;
            public int RendererCount;
            public int SkinnedMeshRendererCount;
            public int ShadowCasterCount;
            public int UpdateWhenOffscreenCount;
            public List<string> SmallShadowCasterNames;
            public List<string> AnimatorCullingModes;
        }

        private struct RendererSearchInfo
        {
            public string HierarchyPath;
            public string MaterialNames;
            public string AssetPaths;
        }

        [Serializable]
        private sealed class ShadowBackup
        {
            public string createdUtc;
            public string scenePath;
            public List<ShadowBackupEntry> renderers;
        }

        [Serializable]
        private sealed class ShadowBackupEntry
        {
            public string globalObjectId;
            public int shadowCastingMode;
            public bool receiveShadows;
        }
    }
}
#endif
