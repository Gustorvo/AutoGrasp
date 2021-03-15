namespace SoftHand
{
    using System;
    using UnityEngine;

    /// <summary>
    /// The Finger class represents a tracked finger.
    /// Fingers are objects that the Leap Motion software has classified as a finger.
    /// Get valid Finger objects from a Frame or a Hand object.
    /// </summary>
    [Serializable]
    public class ArticulatedFinger
    {
        public ArticulatedBone[] bones = new ArticulatedBone[4];

        /// <summary>
        /// Constructs a finger. 
        /// An uninitialized finger is considered invalid.
        /// Get valid Finger objects from a Hand object.       
        /// </summary>
        public ArticulatedFinger()
        {
            bones[0] = new ArticulatedBone();
            bones[1] = new ArticulatedBone();
            bones[2] = new ArticulatedBone();
            bones[3] = new ArticulatedBone();
        }

        /// <summary>
        /// Constructs a finger.   
        /// Generally, you should not create your own finger objects. Such objects will not
        /// have valid tracking data. Get valid finger objects from a hand in a frame
        /// received from the service.  
        /// </summary>
        public ArticulatedFinger(long frameId,
                     int handId,
                     int fingerId,
                     float timeVisible,
                     Vector3 tipPosition,
                     Vector3 direction,
                     float width,
                     float length,
                     bool isExtended,
                     FingerType type,
                     ArticulatedBone metacarpal,
                     ArticulatedBone proximal,
                     ArticulatedBone intermediate,
                     ArticulatedBone distal)
        {
            Type = type;
            bones[0] = metacarpal;
            bones[1] = proximal;
            bones[2] = intermediate;
            bones[3] = distal;
            Id = (handId * 10) + fingerId;
            HandId = handId;
            TipPosition = tipPosition;
            Direction = direction;
            Width = width;
            Length = length;
            IsExtended = isExtended;
            TimeVisible = timeVisible;
        }

        /// <summary>
        /// The bone at a given bone index on this finger.   
        /// </summary>
        public ArticulatedBone Bone(ArticulatedBone.BoneType boneIx)
        {
            return bones[(int)boneIx];
        }

        /// <summary>
        /// A string containing a brief, human readable description of the Finger object. 
        /// </summary>
        public override string ToString()
        {
            return Enum.GetName(typeof(FingerType), Type) + " id:" + Id;
        }

        /// <summary>
        /// The type of this finger.
        /// </summary>
        public ArticulatedFinger.FingerType Type;

        /// <summary>
        /// A unique ID assigned to this Finger object, whose value remains the
        /// same across consecutive frames while the tracked hand remains visible. 
        /// If tracking of the hand is lost, the Leap Motion software may assign a 
        /// new ID when it detects the hand in a future frame.
        /// Use the ID value to find this Finger object in future frames.
        /// IDs should be from 1 to 100 (inclusive). If more than 100 objects are tracked
        /// an IDs of -1 will be used until an ID in the defined range is available. 
        /// </summary>
        public int Id;

        /// <summary>
        /// The Hand associated with a finger.
        /// @since 1.0
        /// </summary>
        public int HandId;

        /// <summary>
        /// The tip position of this Finger.
        /// </summary>
        public Vector3 TipPosition;

        /// <summary>
        /// The direction in which this finger or tool is pointing. The direction is expressed 
        /// as a unit vector pointing in the same direction as the tip.
        /// </summary>
        public Vector3 Direction;

        /// <summary>
        /// The estimated width of the finger.
        /// </summary>
        public float Width;

        /// <summary>
        /// The estimated length of the finger.
        /// </summary>
        public float Length;

        /// <summary>
        /// Whether or not this Finger is in an extended posture.
        /// A finger is considered extended if it is extended straight from the hand as if
        /// pointing. A finger is not extended when it is bent down and curled towards the
        /// palm.
        /// </summary>
        public bool IsExtended;

        /// <summary>
        /// The duration of time this Finger has been visible to the Leap Motion Controller.
        /// </summary>
        public float TimeVisible;

        /// <summary>
        /// Enumerates the names of the fingers.
        /// Members of this enumeration are returned by Finger.Type() to identify a
        /// Finger object.
        /// </summary>
        public enum FingerType
        {
            TYPE_THUMB = 0,
            TYPE_INDEX = 1,
            TYPE_MIDDLE = 2,
            TYPE_RING = 3,
            TYPE_PINKY = 4,
            TYPE_UNKNOWN = -1
        }
    }
}
