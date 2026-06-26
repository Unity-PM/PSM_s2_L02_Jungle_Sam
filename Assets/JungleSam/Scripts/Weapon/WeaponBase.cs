using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponBase : MonoBehaviour
{
    public static event System.Action<WeaponBase, EnemyAI> EnemyHit;

    private static readonly int ShootHash = Animator.StringToHash("Shoot");
    private static readonly int ReloadHash = Animator.StringToHash("Reload");
    private static readonly int ReloadEndHash = Animator.StringToHash("ReloadEnd");
    private static readonly int IsWalkingHash = Animator.StringToHash("IsWalking");
    private static readonly int IsRunningHash = Animator.StringToHash("IsRunning");
    private static readonly int InspectHash = Animator.StringToHash("Inspect");
    private static readonly string[] MuzzlePointFallbackNames =
    {
        "MuzzlePoint",
        "Barrel_end_end",
        "Barrel_end",
        "BarrelEnd",
        "wpn_silencer_end",
        "wpn_silencer",
        "wpn_bullet_end",
        "wpn_bullet",
        "Barrel"
    };

    public WeaponData weaponData;

    protected int _currentAmmo;
    protected int _reserveAmmo;
    protected float _nextFireTime;
    protected Animator _weaponAnimator;
    protected Camera _mainCam;
    protected AudioSource _audioSource;
    protected bool _isReloading;
    protected float _reloadEndTime;
    protected float _shootAnimEndTime;
    protected bool _pendingReload;
    protected bool _lastIsWalking;
    protected bool _lastIsRunning;

    public bool isUnlocked = false;

    [Header("Shot Feedback")]
    [SerializeField] private Transform muzzlePoint;
    [SerializeField] private Transform muzzleRotationReference;
    [SerializeField] private bool useMainCameraForMuzzleFlashRotation = true;
    [SerializeField] private Vector3 muzzleFlashRotationOffset;
    [SerializeField] private AudioSource weaponAudioSource;

    [Header("Raycast")]
    [SerializeField] private LayerMask hitMask;
    [SerializeField] private bool drawDebugRay = true;

    private bool _missingMuzzlePointWarningLogged;

    void Awake()
    {
        _weaponAnimator = GetComponent<Animator>();
        if (_weaponAnimator == null)
            _weaponAnimator = GetComponentInChildren<Animator>();

        CacheMuzzlePoint();
        SetupAudioSource();
    }

    void Start()
    {
        _mainCam = Camera.main;

        if (weaponData == null)
        {
            Debug.LogError("WeaponData not assigned on weapon: " + gameObject.name);
            enabled = false;
            return;
        }

        _currentAmmo = weaponData.maxAmmo;
        _reserveAmmo = weaponData.maxReserveAmmo; // Inicjalizacja schowka

        if (_weaponAnimator == null)
            Debug.LogError("Animator not found on weapon: " + gameObject.name);

        if (weaponData.weaponName == "Pistolet")
            isUnlocked = true;
    }

    void Update()
    {
        if (_isReloading && Time.time >= _reloadEndTime)
            FinishReload();

        if (!_isReloading && _pendingReload && Time.time >= _shootAnimEndTime)
        {
            _pendingReload = false;
            Reload();
        }

        if (!_isReloading && !_pendingReload && Mouse.current != null)
        {
            bool shootInput = weaponData.isAutomatic
                ? Mouse.current.leftButton.isPressed
                : Mouse.current.leftButton.wasPressedThisFrame;

            if (shootInput)
                TryShoot();
        }

        Keyboard keyboard = Keyboard.current;

        if (keyboard == null)
            return;

        if (keyboard.rKey.wasPressedThisFrame && !_isReloading && _currentAmmo < weaponData.maxAmmo)
            Reload();

        if (keyboard.fKey.wasPressedThisFrame && !_isReloading)
        {
            if (_weaponAnimator != null)
            {
                _weaponAnimator.SetTrigger(InspectHash);
            }
        }
    }

    public void TryShoot()
    {
        if (weaponData == null)
            return;

        if (Time.time >= _nextFireTime && _currentAmmo > 0 && !_isReloading)
        {
            Shoot();
            float minTimeBetweenShots = Mathf.Max(weaponData.fireRate, weaponData.shootAnimationDuration);
            _nextFireTime = Time.time + minTimeBetweenShots;
        }
        else if (_currentAmmo == 0 && !_isReloading)
        {
            Reload();
        }
    }

    protected virtual void Shoot()
    {
        _currentAmmo--;
        _shootAnimEndTime = Time.time + weaponData.shootAnimationDuration;

        if (_weaponAnimator != null)
        {
            _weaponAnimator.ResetTrigger(ReloadHash);
            _weaponAnimator.SetTrigger(ShootHash);
        }

        PlayShotFeedbackWithDelay();

        if (_mainCam == null)
            return;

        Ray ray = _mainCam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

        bool hasHit = Physics.Raycast(
            ray,
            out RaycastHit hit,
            weaponData.range,
            hitMask,
            QueryTriggerInteraction.Ignore
        );

        if (drawDebugRay)
        {
            Debug.DrawRay(
                ray.origin,
                ray.direction * (hasHit ? hit.distance : weaponData.range),
                hasHit ? Color.red : Color.green,
                0.25f
            );
        }

        if (hasHit)
        {
            IDamageable damageable = hit.collider.GetComponentInParent<IDamageable>();

            if (damageable != null)
            {
                damageable.TakeDamage(weaponData.damage);
            }
            else
            {
                EnemyAI enemy = hit.collider.GetComponentInParent<EnemyAI>();

                if (enemy != null)
                {
                    enemy.TakeDamage(weaponData.damage);
                    EnemyHit?.Invoke(this, enemy);
                }
            }
        }

        if (_currentAmmo == 0)
            _pendingReload = true;
    }

    public void Reload()
    {
        if (_isReloading || _currentAmmo == weaponData.maxAmmo || _reserveAmmo <= 0)
            return;

        _pendingReload = false;
        _isReloading = true;
        _reloadEndTime = Time.time + weaponData.reloadTime;

        _nextFireTime = _reloadEndTime;

        if (_weaponAnimator != null)
        {
            _weaponAnimator.ResetTrigger(ShootHash);
            _weaponAnimator.SetTrigger(ReloadHash);
        }
    }

    protected void FinishReload()
    {
        if (!_isReloading)
            return;

        _isReloading = false;

        // Reload: przenieś amunicję ze schowka do magazynku
        int ammoNeeded = weaponData.maxAmmo - _currentAmmo;
        int ammoToLoad = Mathf.Min(ammoNeeded, _reserveAmmo);

        _currentAmmo += ammoToLoad;
        _reserveAmmo -= ammoToLoad;

        Debug.Log($"{weaponData.weaponName}: Loaded {ammoToLoad} ammo. Mag: {_currentAmmo}/{weaponData.maxAmmo}, Reserve: {_reserveAmmo}");

        if (_weaponAnimator != null)
        {
            _weaponAnimator.SetTrigger(ReloadEndHash);
            _weaponAnimator.SetBool(IsWalkingHash, _lastIsWalking);
            _weaponAnimator.SetBool(IsRunningHash, _lastIsRunning);
        }
    }

    public void OnReloadAnimationFinished()
    {
        FinishReload();
    }

    public void SetMovementAnimations(bool isWalking, bool isRunning)
    {
        if (_weaponAnimator == null)
            return;

        bool changed = _lastIsWalking != isWalking || _lastIsRunning != isRunning;

        _lastIsWalking = isWalking;
        _lastIsRunning = isRunning;

        // Nie nadpisuj ruchu w trakcie animacji akcji.
        if (!changed || _isReloading || Time.time < _shootAnimEndTime)
            return;

        _weaponAnimator.SetBool(IsWalkingHash, isWalking);
        _weaponAnimator.SetBool(IsRunningHash, isRunning);
    }

    public int GetCurrentAmmo() => _currentAmmo;

    public int GetMaxAmmo() => weaponData.maxAmmo;

    public int GetReserveAmmo() => _reserveAmmo;

    public int GetMaxReserveAmmo() => weaponData.maxReserveAmmo;

    public bool IsReloading() => _isReloading;

    public void AddAmmo(int amount)
    {
        // Dodaj amunicję do schowka, nie do magazynku
        _reserveAmmo = Mathf.Min(_reserveAmmo + amount, weaponData.maxReserveAmmo);
        Debug.Log($"{weaponData.weaponName}: Reserve ammo now {_reserveAmmo}/{weaponData.maxReserveAmmo}");
    }

    private void PlayShotFeedbackWithDelay()
    {
        if (weaponData == null)
            return;

        float flashDelay = Mathf.Max(0f, weaponData.shotFeedbackDelay);
        if (flashDelay <= 0f)
            SpawnMuzzleFlash();
        else
            StartCoroutine(SpawnMuzzleFlashDelayed(flashDelay));

        float soundDelay = weaponData.shotSoundDelay > 0f
            ? weaponData.shotSoundDelay
            : flashDelay;

        if (soundDelay <= 0f)
            PlayShotSound();
        else
            StartCoroutine(PlayShotSoundDelayed(soundDelay));
    }

    private IEnumerator SpawnMuzzleFlashDelayed(float delay)
    {
        yield return new WaitForSeconds(delay);
        SpawnMuzzleFlash();
    }

    private IEnumerator PlayShotSoundDelayed(float delay)
    {
        yield return new WaitForSeconds(delay);
        PlayShotSound();
    }

    private void PlayShotSound()
    {
        if (weaponData == null)
            return;

        AudioClip clip = GetShotSoundClip();
        if (clip == null)
            return;

        SetupAudioSource();

        if (weaponData.interruptPreviousShotSound)
            weaponAudioSource.Stop();

        weaponAudioSource.pitch = RandomRange(weaponData.shootPitchRange, 1f);
        float volume = RandomRange(weaponData.shootVolumeRange, 1f);
        weaponAudioSource.PlayOneShot(clip, volume);
    }

    private void SpawnMuzzleFlash()
    {
        if (weaponData == null || weaponData.muzzleFlashPrefab == null)
            return;

        Transform spawnPoint = GetMuzzleTransform();
        Quaternion flashRotation = GetMuzzleFlashRotation();
        GameObject flash = Instantiate(weaponData.muzzleFlashPrefab, spawnPoint.position, flashRotation);
        MuzzleFlashBillboardEffect billboardEffect =
            flash.GetComponent<MuzzleFlashBillboardEffect>()
            ?? flash.GetComponentInChildren<MuzzleFlashBillboardEffect>();

        if (billboardEffect != null)
        {
            billboardEffect.SetSize(weaponData.muzzleFlashSize);
            billboardEffect.SetCameraDistanceScaling(
                weaponData.scaleMuzzleFlashByCameraDistance,
                weaponData.muzzleFlashReferenceDistance
            );

            if (weaponData.overrideMuzzleFlashTint)
                billboardEffect.SetTint(weaponData.muzzleFlashTint);

            if (weaponData.overrideMuzzleFlashLight)
            {
                billboardEffect.SetLightSettings(
                    weaponData.muzzleFlashLightEnabled,
                    weaponData.muzzleFlashLightLifeTime,
                    weaponData.muzzleFlashLightRange,
                    weaponData.muzzleFlashLightIntensity
                );
            }

            billboardEffect.Initialize(
                spawnPoint,
                muzzleRotationReference,
                useMainCameraForMuzzleFlashRotation,
                muzzleFlashRotationOffset
            );
        }

        bool hasSelfManagedEffect =
            flash.GetComponent<MuzzleFlashEffect>() != null
            || flash.GetComponentInChildren<MuzzleFlashEffect>() != null
            || billboardEffect != null;

        if (!hasSelfManagedEffect)
            Destroy(flash, 0.12f);
    }

    private AudioClip GetShotSoundClip()
    {
        AudioClip[] clips = weaponData.shootSounds;
        if (clips != null && clips.Length > 0)
        {
            int startIndex = Random.Range(0, clips.Length);

            for (int i = 0; i < clips.Length; i++)
            {
                AudioClip clip = clips[(startIndex + i) % clips.Length];
                if (clip != null)
                    return clip;
            }
        }

        return weaponData.shootSound;
    }

    private void SetupAudioSource()
    {
        if (weaponAudioSource == null)
            weaponAudioSource = GetComponent<AudioSource>();

        if (weaponAudioSource == null)
            weaponAudioSource = gameObject.AddComponent<AudioSource>();

        weaponAudioSource.enabled = true;
        weaponAudioSource.playOnAwake = false;
        weaponAudioSource.loop = false;
        weaponAudioSource.spatialBlend = 0f;
        weaponAudioSource.dopplerLevel = 0f;

        _audioSource = weaponAudioSource;
    }

    private void CacheMuzzlePoint()
    {
        if (muzzlePoint != null)
            return;

        foreach (string fallbackName in MuzzlePointFallbackNames)
        {
            Transform found = FindChildRecursive(transform, fallbackName);
            if (found != null)
            {
                muzzlePoint = found;
                return;
            }
        }
    }

    private Transform GetMuzzleTransform()
    {
        if (muzzlePoint != null)
            return muzzlePoint;

        if (!_missingMuzzlePointWarningLogged)
        {
            Debug.LogWarning($"Muzzle point not assigned for weapon '{gameObject.name}'. Falling back to weapon transform.");
            _missingMuzzlePointWarningLogged = true;
        }

        return transform;
    }

    private Quaternion GetMuzzleFlashRotation()
    {
        Quaternion rotation;

        if (useMainCameraForMuzzleFlashRotation && Camera.main != null)
        {
            Transform cameraTransform = Camera.main.transform;
            rotation = Quaternion.LookRotation(cameraTransform.forward, cameraTransform.up);
        }
        else if (muzzleRotationReference != null)
        {
            rotation = Quaternion.LookRotation(muzzleRotationReference.forward, muzzleRotationReference.up);
        }
        else
        {
            rotation = Quaternion.LookRotation(transform.forward, transform.up);
        }

        return rotation * Quaternion.Euler(muzzleFlashRotationOffset);
    }

    private static Transform FindChildRecursive(Transform root, string childName)
    {
        if (root == null)
            return null;

        foreach (Transform child in root)
        {
            if (child.name == childName)
                return child;

            Transform found = FindChildRecursive(child, childName);
            if (found != null)
                return found;
        }

        return null;
    }

    private static float RandomRange(Vector2 range, float fallback)
    {
        if (range.x <= 0f && range.y <= 0f)
            return fallback;

        float min = Mathf.Min(range.x, range.y);
        float max = Mathf.Max(range.x, range.y);
        return Mathf.Approximately(min, max) ? min : Random.Range(min, max);
    }
}
