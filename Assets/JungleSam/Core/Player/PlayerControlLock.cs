using UnityEngine;

[DisallowMultipleComponent]
public class PlayerControlLock : MonoBehaviour
{
    [Header("Behaviours Disabled While Locked")]
    [SerializeField] private MonoBehaviour[] behavioursToDisable;

    private bool _isLocked;
    private bool[] _previousEnabledStates;

    public bool IsLocked => _isLocked;

    private void Awake()
    {
        CacheEnabledStates();
    }

    public void SetLocked(bool locked)
    {
        if (_isLocked == locked)
            return;

        if (behavioursToDisable == null)
        {
            _isLocked = locked;
            return;
        }

        if (!locked)
        {
            RestoreEnabledStates();
            _isLocked = false;
            return;
        }

        CacheEnabledStates();

        foreach (MonoBehaviour behaviour in behavioursToDisable)
        {
            if (behaviour != null && behaviour != this)
                behaviour.enabled = false;
        }

        _isLocked = true;

    }

    private void CacheEnabledStates()
    {
        if (behavioursToDisable == null)
        {
            _previousEnabledStates = null;
            return;
        }

        _previousEnabledStates = new bool[behavioursToDisable.Length];

        for (int i = 0; i < behavioursToDisable.Length; i++)
            _previousEnabledStates[i] = behavioursToDisable[i] != null && behavioursToDisable[i].enabled;
    }

    private void RestoreEnabledStates()
    {
        if (behavioursToDisable == null || _previousEnabledStates == null)
            return;

        int count = Mathf.Min(behavioursToDisable.Length, _previousEnabledStates.Length);

        for (int i = 0; i < count; i++)
        {
            if (behavioursToDisable[i] != null)
                behavioursToDisable[i].enabled = _previousEnabledStates[i];
        }
    }
}
