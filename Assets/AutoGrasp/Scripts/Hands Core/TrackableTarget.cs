using UnityEngine;

namespace SoftHand
{
    public struct TrackableTarget : ITrackable
    {
        public string Name { get; }
        public Pose Pose { get; private set; }
        public Vector3 Velocity { get; private set; }
        public float Speed { get; private set; }
        public Vector3 Position => Pose.position;
        public Quaternion Rotation => Pose.rotation;
        
        public TrackableTarget(string name): this()
        {
            Name = name;
            Pose = new Pose(Vector3.zero, Quaternion.identity);
            Velocity = Vector3.zero;
        }

        public void Update(Pose newPose)
        {
            Vector3 delta = Pose.position - newPose.position;
            Velocity = delta / Time.fixedDeltaTime;
            Speed = delta.magnitude / Time.fixedDeltaTime;
            Pose = newPose;
            // return this;
        }
  
    }
}