using UnityEngine;

public class RespawnPoint : MonoBehaviour
{
    public Vector3 Position => transform.position;
    public Quaternion Rotation => transform.rotation;

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(transform.position, 0.25f);
        Gizmos.DrawLine(transform.position, transform.position + transform.forward * 1.25f);
    }
}
