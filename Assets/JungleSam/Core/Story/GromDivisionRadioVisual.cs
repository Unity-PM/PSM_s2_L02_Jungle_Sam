using TMPro;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
[DisallowMultipleComponent]
public class GromDivisionRadioVisual : MonoBehaviour
{
    private const string GeneratedRootName = "Generated_GromDivisionRadio_Visual";

    [Header("Scale")]
    [SerializeField] private float visualScale = 1f;

    [Header("Materials")]
    [SerializeField] private Color bodyColor = new Color(0.20f, 0.25f, 0.16f);
    [SerializeField] private Color rubberColor = new Color(0.015f, 0.014f, 0.012f);
    [SerializeField] private Color metalColor = new Color(0.10f, 0.095f, 0.08f);
    [SerializeField] private Color screenEmissionColor = new Color(0.42f, 0.9f, 0.5f);
    [SerializeField] private float screenEmissionIntensity = 1.8f;
    [SerializeField] private Color ledEmissionColor = new Color(0.45f, 1f, 0.18f);
    [SerializeField] private float ledEmissionIntensity = 4.5f;

    [Header("Editor")]
    [SerializeField] private bool autoEnsureVisual = false;
    [SerializeField] private bool rebuildOnValidate = false;

    private Transform _generatedRoot;
    private bool _rebuildQueued;

    private void Awake()
    {
        if (autoEnsureVisual)
            EnsureVisual();
    }

    private void OnEnable()
    {
        if (autoEnsureVisual)
            EnsureVisual();
    }

    private void OnValidate()
    {
        if (!autoEnsureVisual)
            return;

        if (!rebuildOnValidate)
            return;

        QueueEditorRebuild();
    }

    [ContextMenu("Rebuild Radio Visual")]
    public void RebuildVisual()
    {
        if (IsPrefabAssetContext())
            return;

        ClearVisual();
        BuildVisual();
    }

    private void EnsureVisual()
    {
        if (IsPrefabAssetContext())
            return;

        if (_generatedRoot == null)
            _generatedRoot = transform.Find(GeneratedRootName);

        if (_generatedRoot == null)
            BuildVisual();
    }

    private void BuildVisual()
    {
        if (IsPrefabAssetContext())
            return;

        GameObject root = new GameObject(GeneratedRootName);
        root.transform.SetParent(transform, false);
        root.transform.localScale = Vector3.one * Mathf.Max(0.01f, visualScale);
        _generatedRoot = root.transform;

        Material body = CreateMaterial("Grom Radio Body", bodyColor, 0f);
        Material rubber = CreateMaterial("Grom Radio Rubber", rubberColor, 0f);
        Material metal = CreateMaterial("Grom Radio Metal", metalColor, 0f);
        Material screen = CreateMaterial("Grom Radio Emissive Screen", screenEmissionColor, screenEmissionIntensity);
        Material led = CreateMaterial("Grom Radio Green LED", ledEmissionColor, ledEmissionIntensity);

        AddCube("Body", new Vector3(0f, 0.42f, 0f), new Vector3(0.46f, 0.72f, 0.16f), body);
        AddCube("Armored_Front_Frame", new Vector3(0f, 0.48f, -0.086f), new Vector3(0.52f, 0.58f, 0.024f), metal);
        AddCube("Front_Panel", new Vector3(0f, 0.48f, -0.105f), new Vector3(0.42f, 0.48f, 0.016f), body);
        AddCube("Screen_Glow", new Vector3(0f, 0.62f, -0.119f), new Vector3(0.28f, 0.13f, 0.012f), screen);
        AddCube("Speaker_Grille", new Vector3(0f, 0.28f, -0.119f), new Vector3(0.32f, 0.17f, 0.012f), metal);

        for (int i = 0; i < 5; i++)
            AddCube($"Speaker_Slit_{i + 1}", new Vector3(0f, 0.23f + i * 0.03f, -0.132f), new Vector3(0.28f, 0.008f, 0.01f), rubber);

        AddButton("Button_CH_Plus", new Vector3(-0.13f, 0.49f, -0.132f), "CH+");
        AddButton("Button_Menu", new Vector3(0f, 0.49f, -0.132f), "MENU");
        AddButton("Button_Scan", new Vector3(0.13f, 0.49f, -0.132f), "SCAN");
        AddButton("Button_CH_Minus", new Vector3(-0.13f, 0.40f, -0.132f), "CH-");
        AddButton("Button_EMG", new Vector3(0f, 0.40f, -0.132f), "EMG");
        AddButton("Button_Lock", new Vector3(0.13f, 0.40f, -0.132f), "LOCK");

        AddCylinder("Antenna_Base", new Vector3(-0.16f, 0.82f, 0f), new Vector3(0.045f, 0.08f, 0.045f), rubber);
        AddCylinder("Antenna", new Vector3(-0.16f, 1.17f, 0f), new Vector3(0.022f, 0.66f, 0.022f), rubber);
        AddCylinder("Knob_Large", new Vector3(0.03f, 0.85f, 0f), new Vector3(0.08f, 0.075f, 0.08f), rubber);
        AddCylinder("Knob_Small", new Vector3(0.16f, 0.83f, 0f), new Vector3(0.06f, 0.055f, 0.06f), rubber);
        AddCylinder("Green_LED", new Vector3(-0.19f, 0.54f, -0.134f), new Vector3(0.025f, 0.01f, 0.025f), led, Quaternion.Euler(90f, 0f, 0f));

        AddCube("Right_Side_Cable", new Vector3(0.31f, 0.49f, 0.01f), new Vector3(0.035f, 0.44f, 0.035f), rubber);
        AddCube("Left_Side_Rail", new Vector3(-0.27f, 0.42f, 0f), new Vector3(0.035f, 0.55f, 0.055f), metal);
        AddCube("Bottom_Reinforcement", new Vector3(0f, 0.08f, -0.02f), new Vector3(0.42f, 0.08f, 0.17f), metal);

        AddLabel("Label_Grom", "GROM DIVISION", new Vector3(0f, 0.75f, -0.136f), 0.038f, Color.black, TextAlignmentOptions.Center);
        AddLabel("Screen_Text", "CH 07\nEMERGENCY", new Vector3(0f, 0.622f, -0.137f), 0.04f, new Color(0.02f, 0.08f, 0.03f), TextAlignmentOptions.Center);
        AddLabel("Bottom_Label", "FIELD RADIO\nWE OPERATE IN SILENCE", new Vector3(0f, 0.11f, -0.134f), 0.026f, new Color(0.04f, 0.04f, 0.03f), TextAlignmentOptions.Center);
    }

    private void AddButton(string name, Vector3 position, string label)
    {
        AddCube(name, position, new Vector3(0.08f, 0.055f, 0.02f), CreateMaterial($"{name}_Material", rubberColor, 0f));
        AddLabel($"{name}_Label", label, position + new Vector3(0f, 0f, -0.017f), 0.022f, Color.gray, TextAlignmentOptions.Center);
    }

    private GameObject AddCube(string name, Vector3 position, Vector3 scale, Material material, Quaternion? rotation = null)
    {
        GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        PreparePrimitive(obj, name, position, scale, material, rotation ?? Quaternion.identity);
        return obj;
    }

    private GameObject AddCylinder(string name, Vector3 position, Vector3 scale, Material material, Quaternion? rotation = null)
    {
        GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        PreparePrimitive(obj, name, position, scale, material, rotation ?? Quaternion.identity);
        return obj;
    }

    private void PreparePrimitive(GameObject obj, string name, Vector3 position, Vector3 scale, Material material, Quaternion rotation)
    {
        obj.name = name;
        obj.transform.SetParent(_generatedRoot, false);
        obj.transform.localPosition = position;
        obj.transform.localRotation = rotation;
        obj.transform.localScale = scale;

        Collider col = obj.GetComponent<Collider>();
        if (col != null)
            DestroyGeneratedObject(col);

        Renderer renderer = obj.GetComponent<Renderer>();
        if (renderer != null)
            renderer.sharedMaterial = material;
    }

    private void AddLabel(string name, string text, Vector3 position, float fontSize, Color color, TextAlignmentOptions alignment)
    {
        GameObject labelObj = new GameObject(name);
        labelObj.transform.SetParent(_generatedRoot, false);
        labelObj.transform.localPosition = position;

        TextMeshPro tmp = labelObj.AddComponent<TextMeshPro>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.color = color;
        tmp.alignment = alignment;
        tmp.textWrappingMode = TextWrappingModes.NoWrap;
        tmp.rectTransform.sizeDelta = new Vector2(0.42f, 0.16f);
    }

    private Material CreateMaterial(string materialName, Color color, float emissionIntensity)
    {
        Material material = new Material(FindCompatibleShader(emissionIntensity > 0f))
        {
            name = materialName
        };

        SetMaterialColor(material, color);

        if (emissionIntensity > 0f && material.HasProperty("_EmissionColor"))
        {
            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", color * emissionIntensity);
        }

        return material;
    }

    private static Shader FindCompatibleShader(bool emissive)
    {
        string[] shaderNames = emissive
            ? new[] { "Universal Render Pipeline/Lit", "Universal Render Pipeline/Unlit", "Universal Render Pipeline/Simple Lit", "Unlit/Color" }
            : new[] { "Universal Render Pipeline/Lit", "Universal Render Pipeline/Simple Lit", "Universal Render Pipeline/Unlit", "Unlit/Color" };

        foreach (string shaderName in shaderNames)
        {
            Shader shader = Shader.Find(shaderName);

            if (shader != null)
                return shader;
        }

        return Shader.Find("Sprites/Default");
    }

    private static void SetMaterialColor(Material material, Color color)
    {
        if (material == null)
            return;

        if (material.HasProperty("_BaseColor"))
            material.SetColor("_BaseColor", color);

        if (material.HasProperty("_Color"))
            material.SetColor("_Color", color);

        material.color = color;
    }

    private void ClearVisual()
    {
        Transform existing = transform.Find(GeneratedRootName);

        if (existing != null)
            DestroyGeneratedObject(existing.gameObject);

        _generatedRoot = null;
    }

    private static void DestroyGeneratedObject(Object obj)
    {
        if (obj == null)
            return;

        if (Application.isPlaying)
            Destroy(obj);
        else
            DestroyImmediate(obj);
    }

    private void QueueEditorRebuild()
    {
#if UNITY_EDITOR
        if (Application.isPlaying || _rebuildQueued || IsPrefabAssetContext())
            return;

        _rebuildQueued = true;

        EditorApplication.delayCall += () =>
        {
            if (this == null)
                return;

            _rebuildQueued = false;
            RebuildVisual();
        };
#else
        EnsureVisual();
#endif
    }

    private bool IsPrefabAssetContext()
    {
#if UNITY_EDITOR
        return !Application.isPlaying && PrefabUtility.IsPartOfPrefabAsset(gameObject);
#else
        return false;
#endif
    }
}
