namespace SoftHand
{
    using System;
    using UnityEngine;

    /// <summary>
    /// The Bone class represents a tracked bone.
    /// All fingers contain 4 bones that make up the anatomy of the finger.
    /// Get valid Bone objects from a Finger object.
    /// Bones are ordered from base to tip, indexed from 0 to 3.  Additionally, the
    /// bone's Type enum may be used to index a specific bone anatomically.
    /// The thumb does not have a base metacarpal bone and therefore contains a valid,
    /// zero length bone at that location. 
    /// </summary>
    [Serializable]
    public class ArticulatedBone : IEquatable<ArticulatedBone>
    {

        /// <summary>
        /// Constructs a default invalid Bone object.      
        /// </summary>
        public ArticulatedBone()
        {
            Type = BoneType.TYPE_INVALID;
        }

        /// <summary>
        /// Constructs a new Bone object.       
        /// </summary>
        public ArticulatedBone(Vector3 prevJoint,
                    Vector3 nextJoint,
                    Vector3 center,
                    Vector3 direction,
                    float length,
                    float width,
                    ArticulatedBone.BoneType type,
                    Quaternion rotation)
        {
            PrevJoint = prevJoint;
            NextJoint = nextJoint;
            Center = center;
            Direction = direction;
            Rotation = rotation;
            Length = length;
            Width = width;
            Type = type;
        }

        /// <summary>
        /// Compare Bone object equality.
        /// Two Bone objects are equal if and only if both Bone objects represent the
        /// exact same physical bone in the same frame and both Bone objects are valid.      
        /// </summary>
        public bool Equals(ArticulatedBone other)
        {
            return Center == other.Center && Direction == other.Direction && Length == other.Length;
        }

        /// <summary>
        /// A string containing a brief, human readable description of the Bone object.       
        /// </summary>
        public override string ToString()
        {
            return Enum.GetName(typeof(BoneType), this.Type) + " bone";
        }

        /// <summary>
        /// The base of the bone, closest to the wrist.
        /// In anatomical terms, this is the proximal end of the bone.    
        /// </summary>
        public Vector3 PrevJoint;

        /// <summary>
        /// The end of the bone, closest to the finger tip.
        /// In anatomical terms, this is the distal end of the bone.    
        /// </summary>
        public Vector3 NextJoint;

        /// <summary>
        /// The midpoint of the bone.    
        /// </summary>
        public Vector3 Center;

        /// <summary>
        /// The normalized direction of the bone from base to tip.      
        /// </summary>
        public Vector3 Direction;

        /// <summary>
        /// The estimated length of the bone.      
        /// </summary>
        public float Length;

        /// <summary>
        /// The average width of the flesh around the bone.       
        /// </summary>
        public float Width;

        /// <summary>
        /// The type of this bone.    
        /// </summary>
        public BoneType Type;

        /// <summary>
        /// The orientation of this Bone as a Quaternion.       
        /// </summary>
        public Quaternion Rotation;

        
        /// <summary>
        /// Enumerates the type of bones.    
        /// Members of this enumeration are returned by Bone.Type() to identify a
        /// Bone object.       
        /// </summary>
        public enum BoneType
        {
            TYPE_INVALID = -1,
            TYPE_METACARPAL = 0,
            TYPE_PROXIMAL = 1,
            TYPE_INTERMEDIATE = 2,
            TYPE_DISTAL = 3
        }
    }
}
