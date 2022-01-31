using System.Collections;
using UnityEngine;

namespace SoftHand
{
    public class SelfSpawner : MonoBehaviour, Ispawnable
    {
        [field: SerializeField, Range(.25f, 5f)] public float RrespawnTime { get; set; } = 2f;
        [field: SerializeField, Range(.1f, .5f)] public float MaxDistance { get; set; } = .3f;

        public float SqrDistanceToInitPos => (transform.position - _initialPose.position).sqrMagnitude;
        public bool IsBeyondMaxDistance => SqrDistanceToInitPos > MaxDistance * MaxDistance;
        public bool IsReachable => SqrDistanceToInitPos < 5f * 5f;

        private Pose _initialPose;
        private bool _oneInstance = true;


        private void Awake() => _initialPose = new Pose(transform.position, transform.rotation);

        public void SetDefaults()
        {
            RrespawnTime = 2f;
            MaxDistance = .3f;
        }

        private void Start() => StartCoroutine(LifetimeRoutine());        

        IEnumerator LifetimeRoutine()
        {
            while (IsReachable)
            {
                if (IsBeyondMaxDistance)
                {
                    Respawn();
                }
                yield return new WaitForSeconds(RrespawnTime);
            }
            Respawn();
            SelfDestroy();
        }

        public void Respawn()
        {
            if (_oneInstance)
            {
                var obj = Instantiate(this, _initialPose.position, _initialPose.rotation, transform.parent);
                obj.name = transform.name;
                _oneInstance = false;
            }
        }
        public void SelfDestroy()
        {
            StopAllCoroutines();
            Destroy(gameObject);
        }
    }
}
