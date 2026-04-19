using UnityEngine;

public class AmmoPack : MonoBehaviour
{
    [Header("Ammo Configuration")]
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
    private Renderer _renderer;
    private Collider _collider;

    void Start()
    {
        _startPosition = transform.position;
        _startRotation = transform.rotation; // ← Zapamiętaj rotację
        _renderer = GetComponent<Renderer>();
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
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null && player.currentWeapon != null)
            {
                // Dodaj amunicję do aktualnie wybranej broni
                player.currentWeapon.AddAmmo(ammoAmount);
                Debug.Log($"Picked up {ammoAmount} ammo!");
                Disable();
            }
        }
    }

    private void Disable()
    {
        _isActive = false;
        _respawnTimer = respawnTime;

        // Ukryj paczkę
        _renderer.enabled = false;
        _collider.enabled = false;

        // Resetuj rotację i pozycję
        transform.rotation = _startRotation;
    }

    private void Respawn()
    {
        _isActive = true;

        // Pokaż paczkę
        _renderer.enabled = true;
        _collider.enabled = true;

        // Resetuj pozycję do startowej
        transform.position = _startPosition;
        transform.rotation = _startRotation;

        Debug.Log("Ammo pack respawned!");
    }

    // Dla debugowania w inspectorze
    public int GetAmmoAmount() => ammoAmount;
    public bool IsActive() => _isActive;
}
