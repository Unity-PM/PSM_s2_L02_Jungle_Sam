using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[DisallowMultipleComponent]
public class EnemyAttackSlotManager : MonoBehaviour
{
    [SerializeField] private int slotsPerRing = 8;
    [SerializeField] private float ringSpacing = 1.2f;
    [SerializeField] private float slotRecheckInterval = 0.75f;
    [SerializeField] private float navMeshSampleRadius = 1.5f;
    [SerializeField] private bool rotateSlotsWithPlayer = false;
    [SerializeField] private bool drawGizmos = true;

    private readonly Dictionary<int, SlotReservation> _reservations = new Dictionary<int, SlotReservation>();
    private readonly List<int> _cleanupIds = new List<int>();
    private float _lastGizmoPreferredDistance = 2f;

    public Vector3 GetOrAssignSlotPosition(Component enemy, Transform target, float preferredDistance)
    {
        if (enemy == null || target == null)
            return target != null ? target.position : transform.position;

        CleanDestroyedReservations();

        int enemyInstanceId = enemy.GetInstanceID();
        preferredDistance = Mathf.Max(0.1f, preferredDistance);
        _lastGizmoPreferredDistance = preferredDistance;

        if (!_reservations.TryGetValue(enemyInstanceId, out SlotReservation reservation))
        {
            reservation = CreateReservation(enemy, enemyInstanceId);
            _reservations[enemyInstanceId] = reservation;
        }

        if (Time.time - reservation.LastUpdateTime >= slotRecheckInterval)
            UpdateReservationPosition(reservation, target, preferredDistance);

        return reservation.LastAssignedPosition;
    }

    public void ReleaseSlot(Component enemy)
    {
        if (enemy == null)
            return;

        _reservations.Remove(enemy.GetInstanceID());
    }

    public bool HasSlot(Component enemy)
    {
        return enemy != null && _reservations.ContainsKey(enemy.GetInstanceID());
    }

    private SlotReservation CreateReservation(Component enemy, int enemyInstanceId)
    {
        int slotIndex = 0;
        int ringIndex = 0;
        FindFirstFreeSlot(out slotIndex, out ringIndex);

        SlotReservation reservation = new SlotReservation
        {
            Enemy = enemy,
            EnemyInstanceId = enemyInstanceId,
            SlotIndex = slotIndex,
            RingIndex = ringIndex,
            LastAssignedPosition = transform.position,
            LastUpdateTime = -slotRecheckInterval
        };

        return reservation;
    }

    private void FindFirstFreeSlot(out int slotIndex, out int ringIndex)
    {
        int safeSlotsPerRing = Mathf.Max(1, slotsPerRing);
        int maxRingsToCheck = Mathf.Max(1, (_reservations.Count / safeSlotsPerRing) + 2);

        for (int ring = 0; ring < maxRingsToCheck; ring++)
        {
            for (int slot = 0; slot < safeSlotsPerRing; slot++)
            {
                if (!IsSlotReserved(slot, ring))
                {
                    slotIndex = slot;
                    ringIndex = ring;
                    return;
                }
            }
        }

        slotIndex = _reservations.Count % safeSlotsPerRing;
        ringIndex = _reservations.Count / safeSlotsPerRing;
    }

    private bool IsSlotReserved(int slotIndex, int ringIndex)
    {
        foreach (SlotReservation reservation in _reservations.Values)
        {
            if (reservation.SlotIndex == slotIndex && reservation.RingIndex == ringIndex)
                return true;
        }

        return false;
    }

    private void UpdateReservationPosition(SlotReservation reservation, Transform target, float preferredDistance)
    {
        Vector3 rawPosition = CalculateSlotPosition(reservation, target, preferredDistance);

        if (NavMesh.SamplePosition(rawPosition, out NavMeshHit hit, navMeshSampleRadius, NavMesh.AllAreas))
            reservation.LastAssignedPosition = hit.position;
        else
            reservation.LastAssignedPosition = target.position;

        reservation.LastUpdateTime = Time.time;
    }

    private Vector3 CalculateSlotPosition(SlotReservation reservation, Transform target, float preferredDistance)
    {
        int safeSlotsPerRing = Mathf.Max(1, slotsPerRing);
        float ringDistance = preferredDistance + ringSpacing * reservation.RingIndex;
        float angle = (360f / safeSlotsPerRing) * reservation.SlotIndex;

        if (rotateSlotsWithPlayer)
            angle += target.eulerAngles.y;

        float radians = angle * Mathf.Deg2Rad;
        Vector3 offset = new Vector3(Mathf.Cos(radians), 0f, Mathf.Sin(radians)) * ringDistance;

        return target.position + offset;
    }

    private void CleanDestroyedReservations()
    {
        _cleanupIds.Clear();

        foreach (KeyValuePair<int, SlotReservation> pair in _reservations)
        {
            if (pair.Value.Enemy == null)
                _cleanupIds.Add(pair.Key);
        }

        for (int i = 0; i < _cleanupIds.Count; i++)
            _reservations.Remove(_cleanupIds[i]);
    }

    private void OnDrawGizmosSelected()
    {
        if (!drawGizmos)
            return;

        int highestRing = 0;

        foreach (SlotReservation reservation in _reservations.Values)
            highestRing = Mathf.Max(highestRing, reservation.RingIndex);

        Gizmos.color = new Color(0f, 0.75f, 1f, 0.35f);
        for (int ring = 0; ring <= highestRing + 1; ring++)
            Gizmos.DrawWireSphere(transform.position, Mathf.Max(0.1f, _lastGizmoPreferredDistance + ringSpacing * ring));

        foreach (SlotReservation reservation in _reservations.Values)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(reservation.LastAssignedPosition, 0.18f);
            Gizmos.DrawLine(transform.position, reservation.LastAssignedPosition);

            Gizmos.color = Color.white;
            Vector3 labelTick = reservation.LastAssignedPosition + Vector3.up * 0.35f;
            Gizmos.DrawLine(reservation.LastAssignedPosition, labelTick);
        }
    }

    private sealed class SlotReservation
    {
        public Component Enemy;
        public int EnemyInstanceId;
        public int SlotIndex;
        public int RingIndex;
        public Vector3 LastAssignedPosition;
        public float LastUpdateTime;
    }
}
