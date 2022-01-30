using SoftHand;
using UnityEngine;

[ExecuteInEditMode]
public class DebugRaycast : MonoBehaviour
{
    [Range(0.1f, 10f)] public float raycastDist = 5f;
    public bool hit;
    public Pose hitPose;
    public ArticulatedHand hand;
    [Range(0.01f, 0.1f)] public float handRadius = 0.05f;

    Vector3 GetHandCenter()
    {
        if (hand && hand.Joints != null)
        {
            return (hand.Joints[5].ArticulationBody.transform.position + hand.ArticulationBody.transform.position) / 2;
        }
        return Vector3.zero;
    }

    private void Update()
    {
        Pose hitTarget = new Pose(transform.position + transform.forward * raycastDist, Quaternion.identity);
        hit = TryCheckSphereToTarget(hitTarget, out Pose newPose);
        if (hit)
        {
            hitPose = newPose;
            return;
        }
        hitPose = new Pose();
    }
    public bool TryCheckSphereToTarget(Pose targetPose, out Pose nearestPose)
    {
        nearestPose = new Pose();
        Vector3 newPosition = Vector3.zero;
        float radius = 0.069f;
        Vector3 direction = targetPose.position - transform.position;
        float distance = direction.magnitude;
        direction.Normalize();
        Vector3 fromPos = transform.position;
        Vector3 toPos = targetPose.position;

        int collisionLayer = GetCollidingLayerMask();
        // collisionLayer = ~collisionLayer;

        //first check if we can freely match the target without hitting anything
        if (!Physics.CheckSphere(toPos, radius, collisionLayer))
        {
            newPosition = toPos;
            nearestPose = new Pose(newPosition, targetPose.rotation);
            return true;
        }
        return false;
    }
    public int GetCollidingLayerMask()
    {
        int _palmLayer = gameObject.layer;
        int _layerMask = 0;
        for (int i = 0; i < 32; i++)
        {
            if (!Physics.GetIgnoreLayerCollision(_palmLayer, i))
            {
                _layerMask = _layerMask | 1 << i;
            }
        }
        return _layerMask;
    }

    private void OnDrawGizmos()
    {
        if (hitPose.position != Vector3.zero)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(hitPose.position, 0.07f);
            Gizmos.DrawRay(transform.position, hitPose.position - transform.position);
        }
        else
        {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(transform.position, transform.forward * raycastDist);
        }
        if (GetHandCenter() != Vector3.zero)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(GetHandCenter(), handRadius);
        }
    }

}
