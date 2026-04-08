using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponBase : MonoBehaviour
{
    public WeaponData weaponData; // Przypisz dane broni w Inspektorze

    protected int _currentAmmo;
    protected float _nextFireTime = 0f;
    protected Animator _weaponAnimator;
    
    public bool isUnlocked = false;
    protected Camera _mainCam;
    
    // Przeładowanie
    protected bool _isReloading = false;
    protected float _reloadTime = 1.5f;
    protected float _reloadEndTime = 0f;

    void Start()
    {
        _mainCam = Camera.main;
        _currentAmmo = weaponData.maxAmmo;
        
        // Znalezienie Animator na broni
        _weaponAnimator = GetComponent<Animator>();
        if (_weaponAnimator == null)
        {
            _weaponAnimator = GetComponentInChildren<Animator>();
        }
        
        // DEBUG
        if (_weaponAnimator != null)
            Debug.Log("✓ Animator znaleziony na broni: " + gameObject.name);
        else
            Debug.LogError("✗ ANIMATOR NIE ZNALEZIONY! Sprawdź hierarchię.");

        if(weaponData.weaponName == "Pistolet")
        {
            isUnlocked = true; // Pistolet jest od razu odblokowany
        }
    }

    void Update()
    {
        // Aktualizacja przeładowania
        if (_isReloading && Time.time >= _reloadEndTime)
        {
            FinishReload();
        }

        // Strzał
        if (Mouse.current.leftButton.isPressed && !_isReloading)
        {
            TryShoot();
        }

        // Przeładowanie (R)
        if (Keyboard.current.rKey.wasPressedThisFrame && !_isReloading && _currentAmmo < weaponData.maxAmmo)
        {
            Reload();
        }
    }

    public void TryShoot()
    {
        // Sprawdzenie: czy można strzelać (cooldown + amunicja + brak przeładowania)
        if (Time.time >= _nextFireTime && _currentAmmo > 0 && !_isReloading)
        {
            Shoot();
            // Ustaw czas następnego strzału na podstawie czasu animacji lub fireRate - którykolwiek jest dłuższy
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
        Debug.Log("Strzał z: " + weaponData.weaponName + " | Amunicja: " + _currentAmmo);

        // Animacja strzału
        if (_weaponAnimator != null)
        {
            Debug.Log("→ Trigger 'Shoot' wysłany do Animator");
            _weaponAnimator.SetTrigger("Shoot");
        }
        else
        {
            Debug.LogError("✗ ANIMATOR NULL w Shoot()!");
        }

        // Dźwięk
        if (weaponData.shootSound != null)
        {
            AudioSource audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
                audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.PlayOneShot(weaponData.shootSound);
        }

        // Efekt muzzle flash
        if (weaponData.muzzleFlashPrefab != null)
        {
            Instantiate(weaponData.muzzleFlashPrefab, transform.position, transform.rotation);
        }

        // Raycast - strzelanie
        Ray ray = _mainCam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));

        if (Physics.Raycast(ray, out RaycastHit hit, weaponData.range))
        {
            // Sprawdź, czy trafiony obiekt ma skrypt EnemyAI
            EnemyAI enemy = hit.transform.GetComponent<EnemyAI>();
            if (enemy != null)
            {
                enemy.TakeDamage(weaponData.damage);
            }
        }
    }

    public void Reload()
    {
        if (_isReloading || _currentAmmo == weaponData.maxAmmo)
            return;

        _isReloading = true;
        _reloadTime = weaponData.reloadTime;
        _reloadEndTime = Time.time + _reloadTime;
        
        // Zablokuj strzały aż do końca przeładowania
        _nextFireTime = _reloadEndTime;
        
        Debug.Log("Przeładowanie: " + weaponData.weaponName);

        if (_weaponAnimator != null)
        {
            _weaponAnimator.SetTrigger("Reload");
        }
    }

    protected void FinishReload()
    {
        _isReloading = false;
        _currentAmmo = weaponData.maxAmmo;
        
        Debug.Log("Przeładowanie ukończone. Amunicja: " + _currentAmmo);

        if (_weaponAnimator != null)
        {
            _weaponAnimator.SetTrigger("ReloadEnd");
        }
    }

    // Metoda wywoływana przez PlayerController do zsynchronizowania animacji ruchu
    public void SetMovementAnimations(bool isWalking, bool isRunning)
    {
        if (_weaponAnimator == null)
        {
            Debug.LogError("✗ ANIMATOR NULL w SetMovementAnimations()!");
            return;
        }

        Debug.Log($"→ Animacja ruchu: Walking={isWalking}, Running={isRunning}");
        _weaponAnimator.SetBool("IsWalking", isWalking);
        _weaponAnimator.SetBool("IsRunning", isRunning);
    }

    public int GetCurrentAmmo()
    {
        return _currentAmmo;
    }

    public int GetMaxAmmo()
    {
        return weaponData.maxAmmo;
    }

    public bool IsReloading()
    {
        return _isReloading;
    }
}