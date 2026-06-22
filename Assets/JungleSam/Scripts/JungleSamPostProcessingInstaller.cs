#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif
using System.IO;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace JungleSam.Rendering
{
#if UNITY_EDITOR
    /// <summary>
    /// One-click installer for Jungle Sam URP post-processing.
    /// Put this file in Assets/JungleSam/Editor/ and run:
    /// Tools/Jungle Sam/Rendering/Create Jungle Post Processing
    /// </summary>
    public static class JungleSamPostProcessingInstaller
    {
        private const string VolumeObjectName = "GlobalVolume_Jungle";
        private const string ProfileFolder = "Assets/JungleSam/Settings/Rendering";
        private const string ProfilePath = ProfileFolder + "/JungleSam_GlobalVolumeProfile.asset";

        [MenuItem("Tools/Jungle Sam/Rendering/Create Jungle Post Processing")]
        public static void CreateJunglePostProcessing()
        {
            EnsureFolder(ProfileFolder);

            VolumeProfile profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(ProfilePath);
            bool profileAlreadyExists = profile != null;
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<VolumeProfile>();
                AssetDatabase.CreateAsset(profile, ProfilePath);
            }
            else if (profile.components.Count > 0)
            {
                bool overwrite = EditorUtility.DisplayDialog(
                    "Jungle Sam Post Processing",
                    "JungleSam_GlobalVolumeProfile already exists. Overwrite it with the default Jungle Sam preset?",
                    "Overwrite",
                    "Cancel"
                );

                if (!overwrite)
                {
                    Debug.Log("[JungleSam] Post-processing setup cancelled. Existing profile was not changed.");
                    return;
                }
            }

            SetupProfile(profile);
            SetupGlobalVolume(profile);
            EnablePostProcessingOnSceneCameras();

            EditorUtility.SetDirty(profile);
            AssetDatabase.SaveAssets();
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

            Debug.Log(profileAlreadyExists
                ? $"[JungleSam] Post-processing updated: {ProfilePath}"
                : $"[JungleSam] Post-processing created: {ProfilePath}");
        }

        private static void SetupProfile(VolumeProfile profile)
        {
            ClearProfile(profile);

            Tonemapping tonemapping = AddVolumeComponent<Tonemapping>(profile);
            tonemapping.mode.Override(TonemappingMode.ACES);

            WhiteBalance whiteBalance = AddVolumeComponent<WhiteBalance>(profile);
            whiteBalance.temperature.Override(8f);
            whiteBalance.tint.Override(-2f);

            ColorAdjustments colorAdjustments = AddVolumeComponent<ColorAdjustments>(profile);
            colorAdjustments.postExposure.Override(0.05f);
            colorAdjustments.contrast.Override(18f);
            colorAdjustments.colorFilter.Override(new Color(1.00f, 0.97f, 0.90f, 1f));
            colorAdjustments.hueShift.Override(0f);
            colorAdjustments.saturation.Override(8f);

            Bloom bloom = AddVolumeComponent<Bloom>(profile);
            bloom.threshold.Override(1.20f);
            bloom.intensity.Override(0.28f);
            bloom.scatter.Override(0.55f);
            bloom.tint.Override(new Color(1.00f, 0.95f, 0.82f, 1f));

            Vignette vignette = AddVolumeComponent<Vignette>(profile);
            vignette.color.Override(Color.black);
            vignette.center.Override(new Vector2(0.5f, 0.5f));
            vignette.intensity.Override(0.18f);
            vignette.smoothness.Override(0.42f);
            vignette.rounded.Override(false);

            FilmGrain filmGrain = AddVolumeComponent<FilmGrain>(profile);
            filmGrain.intensity.Override(0.10f);
            filmGrain.response.Override(0.75f);

            ChromaticAberration chromaticAberration = AddVolumeComponent<ChromaticAberration>(profile);
            chromaticAberration.intensity.Override(0.015f);

            // Celowo nie dodaję Motion Blur i Depth Of Field.
            // W FPS/horde shooter pogarszają czytelność walki i mogą kosztować wydajność.
        }

        private static T AddVolumeComponent<T>(VolumeProfile profile) where T : VolumeComponent
        {
            T component = ScriptableObject.CreateInstance<T>();
            component.name = typeof(T).Name;
            component.active = true;
            component.hideFlags = HideFlags.HideInInspector | HideFlags.HideInHierarchy;

            profile.components.Add(component);
            AssetDatabase.AddObjectToAsset(component, profile);
            EditorUtility.SetDirty(component);

            return component;
        }

        private static void ClearProfile(VolumeProfile profile)
        {
            string assetPath = AssetDatabase.GetAssetPath(profile);
            Object[] assets = AssetDatabase.LoadAllAssetsAtPath(assetPath);

            foreach (Object asset in assets)
            {
                if (asset is VolumeComponent)
                {
                    Object.DestroyImmediate(asset, true);
                }
            }

            profile.components.Clear();
            EditorUtility.SetDirty(profile);
        }

        private static void SetupGlobalVolume(VolumeProfile profile)
        {
            GameObject volumeObject = GameObject.Find(VolumeObjectName);
            if (volumeObject == null)
            {
                volumeObject = new GameObject(VolumeObjectName);
                Undo.RegisterCreatedObjectUndo(volumeObject, "Create Jungle Global Volume");
            }

            Volume volume = volumeObject.GetComponent<Volume>();
            if (volume == null)
            {
                volume = volumeObject.AddComponent<Volume>();
            }

            volume.isGlobal = true;
            volume.priority = 10f;
            volume.weight = 1f;
            volume.sharedProfile = profile;

            EditorUtility.SetDirty(volumeObject);
            EditorUtility.SetDirty(volume);
        }

        private static void EnablePostProcessingOnSceneCameras()
        {
            Camera[] cameras = Object.FindObjectsByType<Camera>(FindObjectsSortMode.None);
            foreach (Camera camera in cameras)
            {
                camera.usePhysicalProperties = false;
                camera.nearClipPlane = Mathf.Min(camera.nearClipPlane, 0.01f);

                UniversalAdditionalCameraData urpCameraData = camera.GetUniversalAdditionalCameraData();
                urpCameraData.renderPostProcessing = true;

                EditorUtility.SetDirty(camera);
                EditorUtility.SetDirty(urpCameraData);
            }
        }

        private static void EnsureFolder(string fullPath)
        {
            string[] parts = fullPath.Split('/');
            string current = parts[0];

            for (int i = 1; i < parts.Length; i++)
            {
                string next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, parts[i]);
                }
                current = next;
            }
        }
    }
#endif
}
