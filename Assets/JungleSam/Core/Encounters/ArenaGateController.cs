using UnityEngine;

[DisallowMultipleComponent]
public class ArenaGateController : MonoBehaviour
{
    [Header("Gate")]
    [SerializeField] private string gateId = "ArenaGate";
    [SerializeField] private bool closedOnStart = false;

    [Header("Objects")]
    [SerializeField] private GameObject[] enableWhenClosed;
    [SerializeField] private GameObject[] disableWhenClosed;

    [Header("Blocking Colliders")]
    [SerializeField] private Collider[] blockingColliders;
    [SerializeField] private bool controlBlockingColliders = true;

    [Header("Optional Animator")]
    [SerializeField] private Animator animator;
    [SerializeField] private string openTrigger = "Open";
    [SerializeField] private string closeTrigger = "Close";
    [SerializeField] private string closedBool = "Closed";

    [Header("Optional Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip openClip;
    [SerializeField] private AudioClip closeClip;

    private bool _isClosed;

    public string GateId => gateId;
    public bool IsClosed => _isClosed;

    private void Reset()
    {
        animator = GetComponentInChildren<Animator>();
        audioSource = GetComponentInChildren<AudioSource>();
        blockingColliders = GetComponentsInChildren<Collider>();
    }

    private void Awake()
    {
        SetClosed(closedOnStart, false);
    }

    [ContextMenu("Open Gate")]
    public void Open()
    {
        SetClosed(false);
    }

    [ContextMenu("Close Gate")]
    public void Close()
    {
        SetClosed(true);
    }

    public void SetClosed(bool closed)
    {
        SetClosed(closed, true);
    }

    private void SetClosed(bool closed, bool playFeedback)
    {
        if (_isClosed == closed && playFeedback)
            return;

        _isClosed = closed;

        SetObjectsActive(enableWhenClosed, closed);
        SetObjectsActive(disableWhenClosed, !closed);

        if (controlBlockingColliders && blockingColliders != null)
        {
            foreach (Collider col in blockingColliders)
            {
                if (col != null)
                    col.enabled = closed;
            }
        }

        UpdateAnimator(closed);

        if (playFeedback)
            PlayAudio(closed);
    }

    private void UpdateAnimator(bool closed)
    {
        if (animator == null)
            return;

        if (!string.IsNullOrWhiteSpace(closedBool))
            animator.SetBool(closedBool, closed);

        string trigger = closed ? closeTrigger : openTrigger;
        string oppositeTrigger = closed ? openTrigger : closeTrigger;

        if (!string.IsNullOrWhiteSpace(oppositeTrigger))
            animator.ResetTrigger(oppositeTrigger);

        if (!string.IsNullOrWhiteSpace(trigger))
            animator.SetTrigger(trigger);
    }

    private void PlayAudio(bool closed)
    {
        if (audioSource == null)
            return;

        AudioClip clip = closed ? closeClip : openClip;

        if (clip != null)
            audioSource.PlayOneShot(clip);
    }

    private static void SetObjectsActive(GameObject[] objects, bool active)
    {
        if (objects == null)
            return;

        foreach (GameObject obj in objects)
        {
            if (obj != null)
                obj.SetActive(active);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = _isClosed ? Color.red : Color.green;
        Gizmos.DrawWireCube(transform.position, Vector3.one);
    }
}
