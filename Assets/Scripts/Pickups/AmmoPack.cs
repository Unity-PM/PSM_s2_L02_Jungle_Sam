using UnityEngine;

public class AmmoPack : MonoBehaviour
{
    [Header("Ammo Configuration")]
    [SerializeField] private AmmoCategory ammoCategory = AmmoCategory.PistolSmg;
    [SerializeField] private int ammoAmount = 9;
    [SerializeField] private float respawnTime = 5f;

    [Header("Animation")]
    [SerializeField] private float rotationSpeed = 45f; // stopnie na sekundę
    [SerializeField] private float bobbingSpeed = 2f;
    [SerializeField] private float bobbingHeight = 0.1f;
    [SerializeField] private bool invertRotation = false; 

    private Vector3 _startPosition;
    private Quaternion _startRotation;
    private bool _isActive = true;
    private float _respawnTimer = 0f;
    private Renderer[] _renderers;
    private Collider _collider;

    void Start()
    {
        _startPosition = transform.position;
        _startRotation = transform.rotation; // ← Zapamiętaj rotację
        _renderers = GetComponentsInChildren<Renderer>(true);
        _collider = GetComponent<Collider>();
        

        if (_collider == null)
            Debug.LogError("AmmoPack: Collider not found!");
    }

    void Update()
    {
        if (_isActive)
        {
            // Rotacja Y względem GLOBALNEJ osi (niezależnie od transformacji prefaba)
            float rotation = invertRotation ? -rotationSpeed : rotationSpeed;
            transform.Rotate(0, rotation * Time.deltaTime, 0, Space.World);

            // Bobbing up-down
            float bobbingOffset = Mathf.Sin(Time.time * bobbingSpeed) * bobbingHeight;
            Vector3 newPos = _startPosition;
            newPos.y += bobbingOffset;
            transform.position = newPos;
        }
        else
        {
            // Respawn countdown
            _respawnTimer -= Time.deltaTime;
            if (_respawnTimer <= 0)
            {
                Respawn();
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!_isActive)
            return;

        if (other.CompareTag("Player"))
        {
            WeaponManager weaponManager = other.GetComponentInChildren<WeaponManager>();
            if (weaponManager == null)
            {
                Debug.LogWarning("AmmoPack: WeaponManager not found on player.");
                return;
            }

            int affectedWeapons = weaponManager.AddAmmoToCategory(ammoCategory, ammoAmount);
            if (affectedWeapons <= 0)
            {
                Debug.Log($"No weapons found for ammo category: {ammoCategory}");
                return;
            }

            Debug.Log($"Picked up {ammoAmount} ammo for {ammoCategory}. Updated weapons: {affectedWeapons}");
            Disable();
        }
    }

    private void Disable()
    {
        _isActive = false;
        _respawnTimer = respawnTime;

        // Ukryj WSZYSTKIE renderery
        foreach (var r in _renderers)
            r.enabled = false;

        _collider.enabled = false;
        transform.rotation = _startRotation;
    }

    private void Respawn()
    {
        _isActive = true;

        // Pokaż WSZYSTKIE renderery
        foreach (var r in _renderers)
            r.enabled = true;

        _collider.enabled = true;

        transform.SetPositionAndRotation(_startPosition, _startRotation);
        Debug.Log("Ammo pack respawned!");
    }

    // Dla debugowania w inspectorze
    public int GetAmmoAmount() => ammoAmount;
    public bool IsActive() => _isActive;
}
