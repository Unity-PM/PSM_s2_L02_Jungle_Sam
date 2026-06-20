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

    [Header("Raycast")]
    [SerializeField] private LayerMask hitMask;
    [SerializeField] private bool drawDebugRay = true;

    void Start()
    {
        _mainCam = Camera.main;
        _currentAmmo = weaponData.maxAmmo;
        _reserveAmmo = weaponData.maxReserveAmmo; // Inicjalizacja schowka

        _weaponAnimator = GetComponent<Animator>();
        if (_weaponAnimator == null)
            _weaponAnimator = GetComponentInChildren<Animator>();

        _audioSource = GetComponent<AudioSource>();
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

        if (weaponData.shootSound != null)
        {
            if (_audioSource == null)
                _audioSource = gameObject.AddComponent<AudioSource>();

            _audioSource.PlayOneShot(weaponData.shootSound);
        }

        if (weaponData.muzzleFlashPrefab != null)
        {
            GameObject flash = Instantiate(
                weaponData.muzzleFlashPrefab,
                transform.position,
                transform.rotation
            );

            Destroy(flash, 1f);
        }

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

        _lastIsWalking = isWalking;
        _lastIsRunning = isRunning;

        // Nie nadpisuj ruchu w trakcie animacji akcji.
        if (_isReloading || Time.time < _shootAnimEndTime)
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
}
