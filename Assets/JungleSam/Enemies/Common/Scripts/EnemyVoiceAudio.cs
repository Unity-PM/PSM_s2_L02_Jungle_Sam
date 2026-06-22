using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
[DisallowMultipleComponent]
public class EnemyVoiceAudio : MonoBehaviour
{
    private static int _activeVoices;

    [Header("Clips")]
    [SerializeField] private AudioClip[] voiceClips;

    [Header("Timing")]
    [SerializeField] private Vector2 voiceDelayRange = new Vector2(4f, 10f);
    [SerializeField] private Vector2 pitchRange = new Vector2(0.92f, 1.08f);

    [Header("3D Audio")]
    [SerializeField] private float volume = 0.55f;
    [SerializeField] private float minDistance = 2f;
    [SerializeField] private float maxDistance = 35f;

    [Header("Limiter")]
    [SerializeField] private int maxGlobalVoices = 7;

    private AudioSource _audioSource;
    private Coroutine _voiceLoopRoutine;
    private Coroutine _voiceReleaseRoutine;
    private Transform _listenerTransform;
    private bool _isDead;
    private bool _voiceCounted;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        ConfigureAudioSource();
    }

    private void OnDisable()
    {
        StopVoiceLoop();
    }

    public void StartVoiceLoop()
    {
        if (_isDead || _voiceLoopRoutine != null)
            return;

        _voiceLoopRoutine = StartCoroutine(VoiceLoop());
    }

    public void StopVoiceLoop()
    {
        if (_voiceLoopRoutine != null)
        {
            StopCoroutine(_voiceLoopRoutine);
            _voiceLoopRoutine = null;
        }

        StopCurrentVoice();
    }

    public void PlayRandomVoice()
    {
        if (_isDead || _audioSource == null || voiceClips == null || voiceClips.Length == 0)
            return;

        if (_audioSource.isPlaying || _voiceCounted)
            return;

        if (_activeVoices >= Mathf.Max(1, maxGlobalVoices))
            return;

        if (IsTooFarFromListener())
            return;

        AudioClip clip = GetRandomClip();
        if (clip == null)
            return;

        ConfigureAudioSource();

        float pitch = Random.Range(Mathf.Min(pitchRange.x, pitchRange.y), Mathf.Max(pitchRange.x, pitchRange.y));
        _audioSource.clip = clip;
        _audioSource.pitch = Mathf.Max(0.01f, pitch);
        _audioSource.volume = Mathf.Clamp01(volume);

        _activeVoices++;
        _voiceCounted = true;

        _audioSource.Play();

        if (_voiceReleaseRoutine != null)
            StopCoroutine(_voiceReleaseRoutine);

        _voiceReleaseRoutine = StartCoroutine(ReleaseVoiceAfter(clip.length / _audioSource.pitch));
    }

    public void SetDead()
    {
        _isDead = true;
        StopVoiceLoop();
    }

    private IEnumerator VoiceLoop()
    {
        while (!_isDead)
        {
            yield return new WaitForSeconds(GetNextDelay());
            PlayRandomVoice();
        }

        _voiceLoopRoutine = null;
    }

    private IEnumerator ReleaseVoiceAfter(float duration)
    {
        yield return new WaitForSeconds(Mathf.Max(0.01f, duration));
        _voiceReleaseRoutine = null;
        ReleaseVoiceCount();
    }

    private void StopCurrentVoice()
    {
        if (_voiceReleaseRoutine != null)
        {
            StopCoroutine(_voiceReleaseRoutine);
            _voiceReleaseRoutine = null;
        }

        if (_audioSource != null)
            _audioSource.Stop();

        ReleaseVoiceCount();
    }

    private void ReleaseVoiceCount()
    {
        if (!_voiceCounted)
            return;

        _voiceCounted = false;
        _activeVoices = Mathf.Max(0, _activeVoices - 1);
    }

    private AudioClip GetRandomClip()
    {
        int startIndex = Random.Range(0, voiceClips.Length);

        for (int i = 0; i < voiceClips.Length; i++)
        {
            AudioClip clip = voiceClips[(startIndex + i) % voiceClips.Length];
            if (clip != null)
                return clip;
        }

        return null;
    }

    private float GetNextDelay()
    {
        float min = Mathf.Max(0.1f, Mathf.Min(voiceDelayRange.x, voiceDelayRange.y));
        float max = Mathf.Max(min, Mathf.Max(voiceDelayRange.x, voiceDelayRange.y));
        return Random.Range(min, max);
    }

    private bool IsTooFarFromListener()
    {
        Transform listener = GetListenerTransform();
        if (listener == null)
            return false;

        float distanceSqr = (listener.position - transform.position).sqrMagnitude;
        float maxDistanceClamped = Mathf.Max(0.1f, maxDistance);
        return distanceSqr > maxDistanceClamped * maxDistanceClamped;
    }

    private Transform GetListenerTransform()
    {
        if (_listenerTransform != null)
            return _listenerTransform;

#if UNITY_2023_1_OR_NEWER
        AudioListener listener = FindFirstObjectByType<AudioListener>();
#else
        AudioListener listener = FindObjectOfType<AudioListener>();
#endif
        _listenerTransform = listener != null ? listener.transform : null;
        return _listenerTransform;
    }

    private void ConfigureAudioSource()
    {
        if (_audioSource == null)
            return;

        _audioSource.playOnAwake = false;
        _audioSource.loop = false;
        _audioSource.spatialBlend = 1f;
        _audioSource.rolloffMode = AudioRolloffMode.Logarithmic;
        _audioSource.minDistance = Mathf.Max(0.01f, minDistance);
        _audioSource.maxDistance = Mathf.Max(_audioSource.minDistance, maxDistance);
        _audioSource.volume = Mathf.Clamp01(volume);
    }
}
