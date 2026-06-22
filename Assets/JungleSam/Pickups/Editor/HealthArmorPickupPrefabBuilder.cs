using System.IO;
using UnityEditor;
using UnityEngine;

public static class HealthArmorPickupPrefabBuilder
{
    private const string PrefabFolder = "Assets/JungleSam/Pickups/Prefabs";
    private const string MaterialFolder = "Assets/JungleSam/Pickups/Materials";

    [MenuItem("Tools/Jungle Sam/Pickups/Build Health And Armor Pickups")]
    public static void BuildHealthAndArmorPickups()
    {
        EnsureFolder("Assets/JungleSam");
        EnsureFolder("Assets/JungleSam/Pickups");
        EnsureFolder(PrefabFolder);
        EnsureFolder(MaterialFolder);

        Material healthRed = CreateOrUpdateMaterial("MAT_Pickup_Health_Red", new Color(0.85f, 0.04f, 0.04f));
        Material healthWhite = CreateOrUpdateMaterial("MAT_Pickup_Health_White", Color.white);
        Material armorBlue = CreateOrUpdateMaterial("MAT_Pickup_Armor_Blue", new Color(0.05f, 0.35f, 0.95f));
        Material armorDark = CreateOrUpdateMaterial("MAT_Pickup_Armor_Dark", new Color(0.02f, 0.06f, 0.14f));

        BuildHealthPrefab("Pickup_HealthSmall", 25f, 25f, 20f, 0.85f, healthRed, healthWhite);
        BuildHealthPrefab("Pickup_HealthLarge", 75f, 50f, 35f, 1.1f, healthRed, healthWhite);
        BuildArmorPrefab("Pickup_ArmorSmall", 25f, 25f, 0.9f, armorBlue, armorDark);
        BuildArmorPrefab("Pickup_ArmorLarge", 50f, 40f, 1.15f, armorBlue, armorDark);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Health and armor pickup prefabs generated.");
    }

    private static void BuildHealthPrefab(
        string prefabName,
        float healthAmount,
        float armorAmount,
        float respawnTime,
        float scale,
        Material redMaterial,
        Material whiteMaterial)
    {
        GameObject root = CreatePickupRoot(prefabName, out Transform visualRoot, out HealthArmorPickup pickup);

        GameObject baseSphere = CreatePrimitiveChild(visualRoot, "Red Capsule", PrimitiveType.Capsule, redMaterial);
        baseSphere.transform.localScale = new Vector3(0.65f, 0.55f, 0.65f) * scale;

        GameObject crossVertical = CreatePrimitiveChild(visualRoot, "Medical Cross Vertical", PrimitiveType.Cube, whiteMaterial);
        crossVertical.transform.localPosition = new Vector3(0f, 0.05f * scale, -0.5f * scale);
        crossVertical.transform.localScale = new Vector3(0.16f, 0.58f, 0.08f) * scale;

        GameObject crossHorizontal = CreatePrimitiveChild(visualRoot, "Medical Cross Horizontal", PrimitiveType.Cube, whiteMaterial);
        crossHorizontal.transform.localPosition = crossVertical.transform.localPosition;
        crossHorizontal.transform.localScale = new Vector3(0.48f, 0.16f, 0.08f) * scale;

        ConfigurePickup(
            pickup,
            HealthArmorPickup.PickupType.Both,
            healthAmount,
            armorAmount,
            true,
            respawnTime,
            visualRoot
        );

        SavePrefabAndCleanup(root, prefabName);
    }

    private static void BuildArmorPrefab(
        string prefabName,
        float armorAmount,
        float respawnTime,
        float scale,
        Material blueMaterial,
        Material darkMaterial)
    {
        GameObject root = CreatePickupRoot(prefabName, out Transform visualRoot, out HealthArmorPickup pickup);

        GameObject shieldBody = CreatePrimitiveChild(visualRoot, "Blue Shield Body", PrimitiveType.Cube, blueMaterial);
        shieldBody.transform.localPosition = new Vector3(0f, 0.02f * scale, 0f);
        shieldBody.transform.localScale = new Vector3(0.7f, 0.95f, 0.18f) * scale;

        GameObject shieldTop = CreatePrimitiveChild(visualRoot, "Blue Shield Top", PrimitiveType.Sphere, blueMaterial);
        shieldTop.transform.localPosition = new Vector3(0f, 0.52f * scale, 0f);
        shieldTop.transform.localScale = new Vector3(0.72f, 0.32f, 0.2f) * scale;

        GameObject shieldCore = CreatePrimitiveChild(visualRoot, "Dark Shield Core", PrimitiveType.Cube, darkMaterial);
        shieldCore.transform.localPosition = new Vector3(0f, 0.1f * scale, -0.13f * scale);
        shieldCore.transform.localScale = new Vector3(0.32f, 0.55f, 0.06f) * scale;

        GameObject shieldRim = CreatePrimitiveChild(visualRoot, "Dark Shield Rim", PrimitiveType.Capsule, darkMaterial);
        shieldRim.transform.localPosition = new Vector3(0f, -0.38f * scale, -0.13f * scale);
        shieldRim.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
        shieldRim.transform.localScale = new Vector3(0.16f, 0.36f, 0.16f) * scale;

        ConfigurePickup(
            pickup,
            HealthArmorPickup.PickupType.Armor,
            0f,
            armorAmount,
            true,
            respawnTime,
            visualRoot
        );

        SavePrefabAndCleanup(root, prefabName);
    }

    private static GameObject CreatePickupRoot(
        string rootName,
        out Transform visualRoot,
        out HealthArmorPickup pickup)
    {
        GameObject root = new GameObject(rootName);
        root.layer = LayerMask.NameToLayer("Default");

        SphereCollider trigger = root.AddComponent<SphereCollider>();
        trigger.isTrigger = true;
        trigger.radius = 1.1f;

        Rigidbody body = root.AddComponent<Rigidbody>();
        body.isKinematic = true;
        body.useGravity = false;

        pickup = root.AddComponent<HealthArmorPickup>();

        GameObject visualObject = new GameObject("VisualRoot");
        visualObject.transform.SetParent(root.transform, false);
        visualRoot = visualObject.transform;

        return root;
    }

    private static GameObject CreatePrimitiveChild(
        Transform parent,
        string name,
        PrimitiveType primitiveType,
        Material material)
    {
        GameObject child = GameObject.CreatePrimitive(primitiveType);
        child.name = name;
        child.transform.SetParent(parent, false);

        Collider collider = child.GetComponent<Collider>();

        if (collider != null)
            Object.DestroyImmediate(collider);

        Renderer renderer = child.GetComponent<Renderer>();

        if (renderer != null)
            renderer.sharedMaterial = material;

        return child;
    }

    private static void ConfigurePickup(
        HealthArmorPickup pickup,
        HealthArmorPickup.PickupType pickupType,
        float healthAmount,
        float armorAmount,
        bool respawn,
        float respawnTime,
        Transform visualRoot)
    {
        SerializedObject serializedPickup = new SerializedObject(pickup);
        serializedPickup.FindProperty("pickupType").enumValueIndex = (int)pickupType;
        serializedPickup.FindProperty("healthAmount").floatValue = healthAmount;
        serializedPickup.FindProperty("armorAmount").floatValue = armorAmount;
        serializedPickup.FindProperty("respawn").boolValue = respawn;
        serializedPickup.FindProperty("respawnTime").floatValue = respawnTime;
        serializedPickup.FindProperty("visualRoot").objectReferenceValue = visualRoot;
        serializedPickup.FindProperty("rotateSpeed").floatValue = 60f;
        serializedPickup.FindProperty("bobAmplitude").floatValue = 0.15f;
        serializedPickup.FindProperty("bobFrequency").floatValue = 2f;
        serializedPickup.ApplyModifiedPropertiesWithoutUndo();
    }

    private static Material CreateOrUpdateMaterial(string materialName, Color color)
    {
        string materialPath = $"{MaterialFolder}/{materialName}.mat";
        Material material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");

        if (shader == null)
            shader = Shader.Find("Standard");

        if (material == null)
        {
            material = new Material(shader);
            AssetDatabase.CreateAsset(material, materialPath);
        }
        else if (shader != null && material.shader != shader)
        {
            material.shader = shader;
        }

        material.name = materialName;
        material.color = color;
        EditorUtility.SetDirty(material);
        return material;
    }

    private static void SavePrefabAndCleanup(GameObject root, string prefabName)
    {
        string prefabPath = $"{PrefabFolder}/{prefabName}.prefab";
        PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
        Object.DestroyImmediate(root);
    }

    private static void EnsureFolder(string folderPath)
    {
        if (AssetDatabase.IsValidFolder(folderPath))
            return;

        string parent = Path.GetDirectoryName(folderPath)?.Replace('\\', '/');
        string folderName = Path.GetFileName(folderPath);

        if (!string.IsNullOrEmpty(parent))
            EnsureFolder(parent);

        AssetDatabase.CreateFolder(parent, folderName);
    }
}
