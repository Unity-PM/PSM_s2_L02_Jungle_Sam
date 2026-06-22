using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class EnemyVoiceAudioPrefabConfigurator
{
    private static readonly string[] PrefabPaths =
    {
        "Assets/JungleSam/Prefabs/Enemies/Zombie/Zombie1.prefab",
        "Assets/JungleSam/Prefabs/Enemies/Zombie/Zombie2.prefab",
        "Assets/JungleSam/Prefabs/Enemies/Monster1.prefab",
        "Assets/JungleSam/Prefabs/Enemies/Monster2.prefab"
    };

    private static readonly string[] VoiceClipNames =
    {
        "zombie_voice_01",
        "zombie_voice_02",
        "zombie_voice_03"
    };

    [MenuItem("Tools/Jungle Sam/Enemies/Configure Enemy Voice Audio")]
    public static void ConfigurePrefabs()
    {
        AudioClip[] clips = LoadVoiceClips();
        int configuredCount = 0;

        foreach (string prefabPath in PrefabPaths)
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath) == null)
            {
                Debug.LogWarning($"Enemy voice configurator skipped missing prefab: {prefabPath}");
                continue;
            }

            GameObject prefabRoot = PrefabUtility.LoadPrefabContents(prefabPath);

            try
            {
                EnemyVoiceAudio voiceAudio = GetOrAddVoiceAudio(prefabRoot);
                ConfigureAudioSource(voiceAudio.GetComponent<AudioSource>());
                ConfigureVoiceAudio(voiceAudio, clips);
                AssignVoiceReference(prefabRoot, voiceAudio);

                PrefabUtility.SaveAsPrefabAsset(prefabRoot, prefabPath);
                configuredCount++;
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(prefabRoot);
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"Enemy voice audio configured on {configuredCount} prefabs. Loaded voice clips: {clips.Length}.");
    }

    private static EnemyVoiceAudio GetOrAddVoiceAudio(GameObject prefabRoot)
    {
        EnemyVoiceAudio voiceAudio = prefabRoot.GetComponent<EnemyVoiceAudio>();

        if (voiceAudio == null)
            voiceAudio = prefabRoot.GetComponentInChildren<EnemyVoiceAudio>(true);

        if (voiceAudio == null)
            voiceAudio = prefabRoot.AddComponent<EnemyVoiceAudio>();

        if (voiceAudio.GetComponent<AudioSource>() == null)
            voiceAudio.gameObject.AddComponent<AudioSource>();

        return voiceAudio;
    }

    private static void ConfigureAudioSource(AudioSource audioSource)
    {
        if (audioSource == null)
            return;

        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.spatialBlend = 1f;
        audioSource.rolloffMode = AudioRolloffMode.Logarithmic;
        audioSource.minDistance = 2f;
        audioSource.maxDistance = 35f;
        audioSource.volume = 0.55f;

        EditorUtility.SetDirty(audioSource);
    }

    private static void ConfigureVoiceAudio(EnemyVoiceAudio voiceAudio, AudioClip[] clips)
    {
        SerializedObject serializedObject = new SerializedObject(voiceAudio);

        SetAudioClipArray(serializedObject.FindProperty("voiceClips"), clips);
        serializedObject.FindProperty("voiceDelayRange").vector2Value = new Vector2(4f, 10f);
        serializedObject.FindProperty("pitchRange").vector2Value = new Vector2(0.92f, 1.08f);
        serializedObject.FindProperty("volume").floatValue = 0.55f;
        serializedObject.FindProperty("minDistance").floatValue = 2f;
        serializedObject.FindProperty("maxDistance").floatValue = 35f;
        serializedObject.FindProperty("maxGlobalVoices").intValue = 7;

        serializedObject.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(voiceAudio);
    }

    private static void AssignVoiceReference(GameObject prefabRoot, EnemyVoiceAudio voiceAudio)
    {
        EnemyAI enemyAI = prefabRoot.GetComponentInChildren<EnemyAI>(true);
        if (enemyAI != null)
            AssignSerializedReference(enemyAI, "enemyVoiceAudio", voiceAudio);

        MutantStalkerAI mutantAI = prefabRoot.GetComponentInChildren<MutantStalkerAI>(true);
        if (mutantAI != null)
            AssignSerializedReference(mutantAI, "enemyVoiceAudio", voiceAudio);
    }

    private static void AssignSerializedReference(Object target, string propertyName, Object reference)
    {
        SerializedObject serializedObject = new SerializedObject(target);
        SerializedProperty property = serializedObject.FindProperty(propertyName);

        if (property == null)
            return;

        property.objectReferenceValue = reference;
        serializedObject.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(target);
    }

    private static void SetAudioClipArray(SerializedProperty property, AudioClip[] clips)
    {
        if (property == null)
            return;

        property.arraySize = clips.Length;

        for (int i = 0; i < clips.Length; i++)
            property.GetArrayElementAtIndex(i).objectReferenceValue = clips[i];
    }

    private static AudioClip[] LoadVoiceClips()
    {
        List<AudioClip> clips = new List<AudioClip>();

        foreach (string clipName in VoiceClipNames)
        {
            AudioClip clip = LoadVoiceClip(clipName);

            if (clip != null)
                clips.Add(clip);
            else
                Debug.LogWarning($"Enemy voice configurator could not find audio clip: {clipName}");
        }

        return clips.ToArray();
    }

    private static AudioClip LoadVoiceClip(string clipName)
    {
        string expectedPath = $"Assets/JungleSam/Audio/Enemies/Zombie/{clipName}.wav";
        AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>(expectedPath);

        if (clip != null)
            return clip;

        string currentPath = $"Assets/JungleSam/Audio/Zombie/{clipName}.wav";
        clip = AssetDatabase.LoadAssetAtPath<AudioClip>(currentPath);

        if (clip != null)
            return clip;

        string[] guids = AssetDatabase.FindAssets($"{clipName} t:AudioClip", new[] { "Assets/JungleSam/Audio" });

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (Path.GetFileNameWithoutExtension(path) != clipName)
                continue;

            return AssetDatabase.LoadAssetAtPath<AudioClip>(path);
        }

        return null;
    }
}
