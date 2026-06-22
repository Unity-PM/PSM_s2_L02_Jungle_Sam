using UnityEngine;
using UnityEngine.Rendering;

public class MuzzleFlashBillboardEffect : MonoBehaviour
{
    [SerializeField] private Texture2D flashTexture;
    [SerializeField] private Color tint = new Color(2.4f, 1.75f, 1.05f, 1f);
    [SerializeField] private Vector2 size = new Vector2(0.65f, 0.35f);
    [SerializeField] private float pivotX = 0.88f;
    [SerializeField] private float pivotY = 0.5f;
    [SerializeField] private bool flipHorizontal = false;
    [SerializeField] private bool faceMainCamera = true;
    [SerializeField] private bool alignTextureToMuzzleDirection = false;
    [SerializeField] private Vector3 muzzleDirectionLocal = Vector3.forward;
    [SerializeField] private bool alignNegativeXToMuzzleDirection = true;
    [SerializeField] private Vector2 randomRollRange = new Vector2(-6f, 6f);
    [SerializeField] private bool createFlashLight = true;
    [SerializeField] private float lightLifeTime = 0.04f;
    [SerializeField] private float lightRange = 1.8f;
    [SerializeField] private float lightIntensity = 4f;
    [SerializeField] private float destroyAfter = 0.08f;

    private Material _runtimeMaterial;
    private Light _flashLight;
    private Transform _quadTransform;
    private float _lightOffTime;
    private float _roll;
    private Transform followTarget;
    private Transform rotationReference;
    private bool useCameraRotation;
    private Vector3 rotationOffset;
    private bool hasRuntimeFollowTarget;
    private bool scaleSizeByCameraDistance;
    private float cameraDistanceReference = 1f;

    public void Initialize(
        Transform followTarget,
        Transform rotationReference,
        bool useCameraRotation,
        Vector3 rotationOffset)
    {
        this.followTarget = followTarget;
        this.rotationReference = rotationReference;
        this.useCameraRotation = useCameraRotation;
        this.rotationOffset = rotationOffset;
        hasRuntimeFollowTarget = followTarget != null;

        UpdateTransformToTarget();
    }

    public void SetSize(Vector2 newSize)
    {
        if (Mathf.Approximately(newSize.x, 0f) || Mathf.Approximately(newSize.y, 0f))
            return;

        size = new Vector2(Mathf.Abs(newSize.x), Mathf.Abs(newSize.y));
        ApplyQuadLayout();
    }

    public void SetCameraDistanceScaling(bool enabled, float referenceDistance)
    {
        scaleSizeByCameraDistance = enabled;
        cameraDistanceReference = Mathf.Max(0.01f, referenceDistance);
        ApplyQuadLayout();
    }

    public void SetTint(Color newTint)
    {
        tint = newTint;

        if (_runtimeMaterial == null)
            return;

        SetColorIfPresent(_runtimeMaterial, "_BaseColor", tint);
        SetColorIfPresent(_runtimeMaterial, "_Color", tint);
    }

    public void SetLightSettings(bool enabled, float lifeTime, float range, float intensity)
    {
        createFlashLight = enabled;
        lightLifeTime = Mathf.Max(0f, lifeTime);
        lightRange = Mathf.Max(0f, range);
        lightIntensity = Mathf.Max(0f, intensity);

        if (!enabled)
        {
            if (_flashLight != null)
                _flashLight.enabled = false;

            return;
        }

        if (_flashLight == null)
            CreateLight();

        ApplyLightSettings();

        if (_flashLight != null)
        {
            _flashLight.enabled = true;
            _lightOffTime = Time.time + lightLifeTime;
        }
    }

    private void Awake()
    {
        _roll = Random.Range(randomRollRange.x, randomRollRange.y);
        CreateQuad();
        CreateLight();
    }

    private void OnEnable()
    {
        if (_flashLight != null)
        {
            ApplyLightSettings();
            _flashLight.enabled = true;
            _lightOffTime = Time.time + lightLifeTime;
        }

        Destroy(gameObject, destroyAfter);
    }

    private void LateUpdate()
    {
        if (hasRuntimeFollowTarget)
            UpdateTransformToTarget();

        if (!hasRuntimeFollowTarget && faceMainCamera && Camera.main != null)
        {
            Transform cameraTransform = Camera.main.transform;
            transform.rotation = GetBillboardRotation(cameraTransform);
            transform.Rotate(Vector3.forward, _roll, Space.Self);
        }

        ApplyQuadLayout();

        if (_flashLight != null && _flashLight.enabled && Time.time >= _lightOffTime)
            _flashLight.enabled = false;
    }

    private void OnDestroy()
    {
        if (_runtimeMaterial != null)
            Destroy(_runtimeMaterial);
    }

    private void CreateQuad()
    {
        GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        quad.name = "MuzzleFlashTextureQuad";
        quad.transform.SetParent(transform, false);
        _quadTransform = quad.transform;
        ApplyQuadLayout();

        Collider quadCollider = quad.GetComponent<Collider>();
        if (quadCollider != null)
            Destroy(quadCollider);

        MeshRenderer meshRenderer = quad.GetComponent<MeshRenderer>();
        if (meshRenderer == null)
            return;

        _runtimeMaterial = CreateMaterial();
        meshRenderer.sharedMaterial = _runtimeMaterial;
        meshRenderer.shadowCastingMode = ShadowCastingMode.Off;
        meshRenderer.receiveShadows = false;
        meshRenderer.lightProbeUsage = LightProbeUsage.Off;
        meshRenderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
    }

    private void ApplyQuadLayout()
    {
        if (_quadTransform == null)
            return;

        Vector2 sanitizedSize = GetSanitizedSize() * GetCameraDistanceScale();
        _quadTransform.localPosition = GetPivotOffset(sanitizedSize);
        _quadTransform.localRotation = faceMainCamera ? Quaternion.identity : Quaternion.Euler(0f, 0f, _roll);
        _quadTransform.localScale = new Vector3(flipHorizontal ? -sanitizedSize.x : sanitizedSize.x, sanitizedSize.y, 1f);
    }

    private Vector2 GetSanitizedSize()
    {
        return new Vector2(Mathf.Max(0.001f, Mathf.Abs(size.x)), Mathf.Max(0.001f, Mathf.Abs(size.y)));
    }

    private Vector3 GetPivotOffset(Vector2 sanitizedSize)
    {
        float x = (0.5f - Mathf.Clamp01(pivotX)) * sanitizedSize.x;
        float y = (0.5f - Mathf.Clamp01(pivotY)) * sanitizedSize.y;
        return new Vector3(flipHorizontal ? -x : x, y, 0f);
    }

    private float GetCameraDistanceScale()
    {
        if (!scaleSizeByCameraDistance || Camera.main == null)
            return 1f;

        Transform cameraTransform = Camera.main.transform;
        Vector3 toEffect = transform.position - cameraTransform.position;
        float distanceAlongView = Vector3.Dot(toEffect, cameraTransform.forward);

        if (distanceAlongView <= 0.01f)
            distanceAlongView = toEffect.magnitude;

        return Mathf.Clamp(distanceAlongView / cameraDistanceReference, 0.05f, 4f);
    }

    private void CreateLight()
    {
        if (!createFlashLight)
            return;

        _flashLight = gameObject.AddComponent<Light>();
        _flashLight.type = LightType.Point;
        _flashLight.shadows = LightShadows.None;
        _flashLight.color = new Color(1f, 0.78f, 0.45f);
        _flashLight.enabled = false;
        ApplyLightSettings();
    }

    private void ApplyLightSettings()
    {
        if (_flashLight == null)
            return;

        _flashLight.range = lightRange;
        _flashLight.intensity = lightIntensity;
        _flashLight.color = new Color(1f, 0.78f, 0.45f);
        _flashLight.shadows = LightShadows.None;
    }

    private void UpdateTransformToTarget()
    {
        if (followTarget == null)
            return;

        transform.position = followTarget.position;

        Quaternion targetRotation;
        if (useCameraRotation && Camera.main != null)
        {
            Transform cameraTransform = Camera.main.transform;
            targetRotation = Quaternion.LookRotation(cameraTransform.forward, cameraTransform.up);
        }
        else if (rotationReference != null)
        {
            targetRotation = Quaternion.LookRotation(rotationReference.forward, rotationReference.up);
        }
        else
        {
            targetRotation = transform.rotation;
        }

        transform.rotation = targetRotation * Quaternion.Euler(rotationOffset);
    }

    private Quaternion GetBillboardRotation(Transform cameraTransform)
    {
        Vector3 viewForward = transform.position - cameraTransform.position;
        if (viewForward.sqrMagnitude < 0.0001f)
            return transform.rotation;

        viewForward.Normalize();

        if (!alignTextureToMuzzleDirection)
            return Quaternion.LookRotation(viewForward, cameraTransform.up);

        Vector3 muzzleDirection = GetMuzzleDirection();
        Vector3 projectedDirection = Vector3.ProjectOnPlane(muzzleDirection, viewForward);
        if (projectedDirection.sqrMagnitude < 0.0001f)
            return Quaternion.LookRotation(viewForward, cameraTransform.up);

        projectedDirection.Normalize();

        Vector3 xAxis = alignNegativeXToMuzzleDirection ? -projectedDirection : projectedDirection;
        Vector3 yAxis = Vector3.Cross(viewForward, xAxis);
        if (yAxis.sqrMagnitude < 0.0001f)
            return Quaternion.LookRotation(viewForward, cameraTransform.up);

        return Quaternion.LookRotation(viewForward, yAxis.normalized);
    }

    private Vector3 GetMuzzleDirection()
    {
        Vector3 localDirection = muzzleDirectionLocal.sqrMagnitude > 0.0001f
            ? muzzleDirectionLocal.normalized
            : Vector3.forward;

        Transform source = transform.parent != null ? transform.parent : transform;
        return source.TransformDirection(localDirection).normalized;
    }

    private Material CreateMaterial()
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
        if (shader == null)
            shader = Shader.Find("Universal Render Pipeline/Unlit");
        if (shader == null)
            shader = Shader.Find("Unlit/Transparent");
        if (shader == null)
            shader = Shader.Find("Sprites/Default");
        if (shader == null)
            shader = Shader.Find("Standard");

        Material material = new Material(shader)
        {
            name = "Runtime Muzzle Flash Texture Material"
        };

        SetTextureIfPresent(material, "_BaseMap", flashTexture);
        SetTextureIfPresent(material, "_MainTex", flashTexture);
        SetColorIfPresent(material, "_BaseColor", tint);
        SetColorIfPresent(material, "_Color", tint);
        SetFloatIfPresent(material, "_Surface", 1f);
        SetFloatIfPresent(material, "_Blend", 1f);
        SetFloatIfPresent(material, "_SrcBlend", (float)BlendMode.SrcAlpha);
        SetFloatIfPresent(material, "_DstBlend", (float)BlendMode.One);
        SetFloatIfPresent(material, "_ZWrite", 0f);
        SetFloatIfPresent(material, "_Cull", (float)CullMode.Off);

        material.SetOverrideTag("RenderType", "Transparent");
        material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        material.DisableKeyword("_ALPHATEST_ON");
        material.EnableKeyword("_ALPHABLEND_ON");
        material.renderQueue = (int)RenderQueue.Transparent;

        return material;
    }

    private static void SetTextureIfPresent(Material material, string propertyName, Texture texture)
    {
        if (texture != null && material.HasProperty(propertyName))
            material.SetTexture(propertyName, texture);
    }

    private static void SetColorIfPresent(Material material, string propertyName, Color color)
    {
        if (material.HasProperty(propertyName))
            material.SetColor(propertyName, color);
    }

    private static void SetFloatIfPresent(Material material, string propertyName, float value)
    {
        if (material.HasProperty(propertyName))
            material.SetFloat(propertyName, value);
    }
}
