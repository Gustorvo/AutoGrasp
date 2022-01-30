using NaughtyAttributes;
using SoftHand;
using System;
using UnityEngine;

namespace SoftHand
{
    [CreateAssetMenu(menuName = "SoftHand/Create new hand configuration")]
    [System.Serializable]
    public class HandConfig : ScriptableObject, IHandConfig
    {
        [SerializeField] ArticulationBodyConfig palmConfig = new ArticulationBodyConfig();
        [SerializeField] ArticulationBodyConfig jointConfig = new ArticulationBodyConfig();

        public IBodyConfig Palm => palmConfig;
        public IBodyConfig Joint => jointConfig;       
    }

    [System.Serializable]
    public class ArticulationBodyConfig : IBodyConfig
    {
        [SerializeField] float mass = 1f;
        [SerializeField] bool useGravity = true;
        [SerializeField] float linearDamping = 0.025f;
        [SerializeField] float angularDamping = 0.025f;
        [SerializeField] float jointFriciton = 0.025f;
        [SerializeField] CollisionDetectionMode collisionDetection = CollisionDetectionMode.ContinuousDynamic;
        [SerializeField] float maxAngularVelocity = 2f;
        [SerializeField] float maxLinearVelocity = 2f;
        [SerializeField] float maxDepenetrationVelocity = 1.5f;
        public float Mass => mass;
        public bool UseGravity => useGravity;
        public float LinearDamping => linearDamping;
        public float AngularDamping => angularDamping;
        public float JointFriciton => jointFriciton;
        public CollisionDetectionMode CollisionDetection => collisionDetection;
        public float MaxAngularVelocity => maxAngularVelocity;
        public float MaxLinearVelocity => maxLinearVelocity;
        public float MaxDepenetrationVelocity => maxDepenetrationVelocity;
    }
}