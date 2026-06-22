using UnityEngine;

public class MuzzleFlashEffect : MonoBehaviour
{
    [SerializeField] private ParticleSystem[] particleSystems;
    [SerializeField] private Light flashLight;
    [SerializeField] private float lightLifeTime = 0.04f;
    [SerializeField] private float destroyAfter = 0.15f;

    private float _lightOffTime;

    private void Awake()
    {
        if (particleSystems == null || particleSystems.Length == 0)
            particleSystems = GetComponentsInChildren<ParticleSystem>(true);

        if (flashLight == null)
            flashLight = GetComponentInChildren<Light>(true);
    }

    private void OnEnable()
    {
        PlayParticles();

        if (flashLight != null)
        {
            flashLight.enabled = true;
            _lightOffTime = Time.time + lightLifeTime;
        }

        if (destroyAfter > 0f)
            Destroy(gameObject, destroyAfter);
    }

    private void Update()
    {
        if (flashLight != null && flashLight.enabled && Time.time >= _lightOffTime)
            flashLight.enabled = false;
    }

    private void PlayParticles()
    {
        if (particleSystems == null)
            return;

        foreach (ParticleSystem particleSystem in particleSystems)
        {
            if (particleSystem == null)
                continue;

            particleSystem.Clear(true);
            particleSystem.Play(true);
        }
    }
}
