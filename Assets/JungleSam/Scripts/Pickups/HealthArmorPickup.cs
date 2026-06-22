using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public class HealthArmorPickup : MonoBehaviour
{
    public enum PickupType
    {
        Health,
        Armor,
        Both
    }

    [Header("Pickup")]
    [SerializeField] private PickupType pickupType = PickupType.Health;
    [SerializeField] private float healthAmount = 25f;
    [SerializeField] private float armorAmount = 25f;
    [SerializeField] private bool respawn = true;
    [SerializeField] private float respawnTime = 20f;

    [Header("Visual")]
    [SerializeField] private Transform visualRoot;
    [SerializeField] private float rotateSpeed = 60f;
    [SerializeField] private float bobAmplitude = 0.15f;
    [SerializeField] private float bobFrequency = 2f;

    [Header("Feedback")]
    [SerializeField] private AudioClip pickupSound;
    [SerializeField] private GameObject pickupVfx;

    private Renderer[] _renderers;
    private Collider _collider;
    private Vector3 _visualStartLocalPosition;
    private bool _available = true;

    private void Awake()
    {
        if (visualRoot == null)
            visualRoot = transform;

        _renderers = visualRoot.GetComponentsInChildren<Renderer>(true);
        _collider = GetComponent<Collider>();
        _visualStartLocalPosition = visualRoot.localPosition;

        if (_collider == null)
            Debug.LogError($"[{name}] HealthArmorPickup requires a trigger Collider.");
        else if (!_collider.isTrigger)
            _collider.isTrigger = true;
    }

    private void Update()
    {
        if (!_available || visualRoot == null)
            return;

        visualRoot.Rotate(0f, rotateSpeed * Time.deltaTime, 0f, Space.World);

        float bobOffset = Mathf.Sin(Time.time * bobFrequency) * bobAmplitude;
        visualRoot.localPosition = _visualStartLocalPosition + Vector3.up * bobOffset;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!_available)
            return;

        PlayerStats playerStats = other.GetComponentInParent<PlayerStats>();

        if (playerStats == null)
            return;

        bool used = ApplyPickup(playerStats);

        if (!used)
            return;

        PlayFeedback();
        ConsumePickup();
    }

    private bool ApplyPickup(PlayerStats playerStats)
    {
        bool used = false;

        if (pickupType == PickupType.Health || pickupType == PickupType.Both)
            used |= playerStats.Heal(healthAmount);

        if (pickupType == PickupType.Armor || pickupType == PickupType.Both)
            used |= playerStats.AddArmor(armorAmount);

        return used;
    }

    private void PlayFeedback()
    {
        if (pickupSound != null)
            AudioSource.PlayClipAtPoint(pickupSound, transform.position);

        if (pickupVfx != null)
            Instantiate(pickupVfx, transform.position, transform.rotation);
    }

    private void ConsumePickup()
    {
        if (!respawn)
        {
            Destroy(gameObject);
            return;
        }

        StartCoroutine(RespawnRoutine());
    }

    private IEnumerator RespawnRoutine()
    {
        SetAvailable(false);
        yield return new WaitForSeconds(respawnTime);
        SetAvailable(true);
    }

    private void SetAvailable(bool available)
    {
        _available = available;

        if (_collider != null)
            _collider.enabled = available;

        for (int i = 0; i < _renderers.Length; i++)
            _renderers[i].enabled = available;

        if (visualRoot != null)
            visualRoot.localPosition = _visualStartLocalPosition;
    }
}
